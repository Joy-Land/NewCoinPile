/*
功能描述：
    本模块用于维护StreamProviderBase，是StreamProvider族的基类
注意事项：
    
代办事项：
    
主要功能：
    1. attach/detach 维护自身reference计数
    2. on awake/on sleep 维护全局真实stream资源
    3. OpenReadByParameter 维护stream的不同打开方式（file、bsa）
    
引用计数：
    1. 计数应只在主线程进行，子线程不应影响计数
    2. Attach()     每次+1
    3. Provide()    中识别首次访问的话，会对计数+1，跟Attach相似
    4. Detach()     每次-1，如果出现小于0的情况视为Error
    5. Dispose()    循环-1直到0，然后释放资源
    
时间更新：
    1. OnAwake      计数从0变到1，会触发OnAwake回调（注意不是新建stream触发，ThreadBindStreamProvider大多时候并不会新建stream）
    2. Seek         频率不高，但一般连续读取之前都会触发，满足更新时间戳的需求
    
数据监控：
    1. DisposeImmediately ==  true： 打开的文件流总数量FileStreamCount应当能快速恢复到0
    2. DisposeImmediately == false： ThreadBindStreamProvider下，文件流总数量FileStreamCount大多时候应大于等于19为正常（考虑19线程+外部文件）

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using UnityEngine;

namespace ThinRL.Core.FileSystem
{
    public abstract class StreamProviderBase : IAttachableStreamProvider
    {
        // sleep时候的位置
        long m_SleepPosition;

        // 引用计数
        private int m_ReferenceCount;
        public int referenceCount => m_ReferenceCount;

        // 最后访问时间，DisposeImmediately为false时候，可以结合最后访问时间进行释放
        private int m_LastAccessEnvironmentTickCount;

        // 引用计数为0时，直接释放
        public static bool DisposeImmediately { get; set; } = false;

        /// <summary>
        /// 关闭时间，多久没访问过即可关闭（如果DisposeImmediately==false，那么多久没访问之后可以关闭文件流）
        /// </summary>
        static double s_DefaultCouldCloseStreamTime = 10 * 1000;  // milliseconds

        /// <summary>
        /// 记录所有StreamProviderBase实例，延时释放时，用来遍历每个实例，检查其是否超时
        /// </summary>
        static readonly HashSet<StreamProviderBase> s_StreamProviderCache = new HashSet<StreamProviderBase>();

        /// <summary>
        /// 延时释放stream的检查时间间隔，现在每次load asset完都会触发检测，性能堪忧，所以设置间隔，减少检查频率
        /// 通过链表将最新的放到最前或最后（lru）？
        /// </summary>
        static double s_DefaultCheckCloseStreamTime = 3 * 1000;  // milliseconds

        /// <summary>
        /// 上一次检查关闭的时间
        /// 与s_DefaultCheckCloseStreamTime配合使用
        /// </summary>
        static long s_LastCloseStreamTime = 0;  // ms: Environment.TickCount

        protected StreamProviderBase() : this(default)
        {
        }

        protected StreamProviderBase(OpenReadParameter parameter)
        {
            this.parameter = parameter;

            s_StreamProviderCache.Add(this);
        }

        #region IStreamProvider
        // provider，默认为OpenReadByParameter，也支持上层指定新的方式作为扩展
        // PS：后面如果觉得耦合，可以考虑由上层进行默认赋值
        public virtual DelegateOpenRead delegateOpenRead { get; set; } = FileUtil.OpenReadByParameter;

        public OpenReadParameter parameter { get; set; }

        public Stream Provide()
        {
            // 尝试获得已打开的stream
            Stream result;

            // 没有引用的时候，会自动attach并引用一下，可以简化上层使用难度
            // 比如BundleEncryptorUtility.GetDecryptStream里面，创建出provider之后，不再需要主动attach
            // old:
            // - Provider provider = new Provider();
            // - provider.Attach();
            // - provider...
            //
            // new:
            // + Provider provider = new Provider();
            // + provider...
            if (m_ReferenceCount == 0)
            {
                result = AttachInternal();
            }
            else
            {
                result = OpenRead(false);

                // 如果没有（意外关闭或尚未首次打开），那么尝试打开一个
                // 应当是ThreadBindStreamProvider的情况才可能发生，因为子线程可能尚未初始化过对应线程的stream
                // 且不应影响计数
                if (result == null)
                {
                    result = OpenRead(true);

                    // 调试完毕需屏蔽
                    //Debug.Assert(result.ToString().EndsWithQuickly(".apk"));
                }
            }

            Debug.Assert(m_ReferenceCount > 0);

            return result;
        }

        protected virtual Stream OpenRead(bool tryCreateIfNotExists)
        {
            Stream result = default;

            if (tryCreateIfNotExists)
            {
                result = delegateOpenRead.Invoke(parameter);

                result?.Seek(m_SleepPosition, SeekOrigin.Begin);
            }

            return result;
        }

        public virtual bool Valid()
        {
            return ValidStream() || ValidProvider();
        }

        public virtual bool ValidStream()
        {
            return OpenRead(false) != null && m_ReferenceCount != 0;
        }

        public virtual bool ValidProvider()
        {
            return parameter.Valid();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                parameter = default;

                DisposeStream();

                s_StreamProviderCache.Remove(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion

        #region IAttachable
        public virtual void Attach()
        {
            AttachInternal();
        }

        protected virtual Stream AttachInternal()
        {
            var result = OpenRead(true);

            IncreaseReferenceCount();

            if (m_ReferenceCount == 1)
            {
                OnAwake();
            }

            return result;
        }

        public virtual void Detach()
        {
            DecreaseReferenceCount();

            if (m_ReferenceCount == 0 && DisposeImmediately)
            {
                RecordStreamPositionWhenClosed();

                OnSleep();
            }
        }

        public virtual void DetachOutOfTime()
        {
            if (m_ReferenceCount == 0 && !DisposeImmediately && (Environment.TickCount - m_LastAccessEnvironmentTickCount >= s_DefaultCouldCloseStreamTime))
            {
                RecordStreamPositionWhenClosed();

                OnSleep();
            }
        }

        /// <summary>
        /// 更新访问时间戳(现在设计为seek时候的时间，防止不必要的时间消耗，以10w次计)
        /// </summary>
        public virtual void UpdateLastEnvironmentTickCount()
        {
            //if (!DisposeImmediately)
            {
                UnityEngine.Profiling.Profiler.BeginSample("UpdateLastAccessDateTimeTicks");
                // https://docs.microsoft.com/en-us/dotnet/api/system.environment.tickcount?view=net-5.0
                // The value of this property is derived from the system timer and is stored as a 32-bit signed integer.
                // Note that, because it is derived from the system timer, the resolution of the TickCount property
                // is limited to the resolution of the system timer, which is typically in the range of 10 to 16 milliseconds.
                Interlocked.Exchange(ref m_LastAccessEnvironmentTickCount, Environment.TickCount);
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public virtual long GetLastEnvironmentTickCount()
        {
            return m_LastAccessEnvironmentTickCount;
        }
        #endregion

        protected void IncreaseReferenceCount()
        {
            Interlocked.Increment(ref m_ReferenceCount);
        }

        protected void DecreaseReferenceCount()
        {
            Interlocked.Decrement(ref m_ReferenceCount);

            if (m_ReferenceCount < 0)
            {
                Debug.LogError($"asset bundle info: 释放次数过多，当前引用计数：{m_ReferenceCount}，frame count: {Time.frameCount}, ticks {System.DateTime.Now.Ticks}\nstream：{OpenRead(false)?.Name()}");
            }
        }

        // 尝试释放延时释放的stream
        public static void TryReleaseStreamOutOfTime()
        {
            UnityEngine.Profiling.Profiler.BeginSample("TryReleaseStream");
            // 如果是直接释放，那么detach的时候应能直接释放完成，直接返回
            if (StreamProviderBase.DisposeImmediately)
            {
                return;
            }

            // 超过检测间隔，才进行检测
            if (Environment.TickCount - s_LastCloseStreamTime >= s_DefaultCheckCloseStreamTime)
            {
                s_LastCloseStreamTime = Environment.TickCount;

                foreach (var provider in s_StreamProviderCache)
                {
                    provider.DetachOutOfTime();
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        // 记录释放时候的position
        void RecordStreamPositionWhenClosed()
        {
            var stream = OpenRead(false);

            if (stream != null)
            {
                m_SleepPosition = stream.Position;
            }
        }

        // 释放stream资源
        void DisposeStream()
        {
            while (m_ReferenceCount > 0)
            {
                Detach();
            }

            if (m_ReferenceCount != 0)
            {
                Debug.LogError($"asset bundle info: 资源释放过程中被使用，当前引用计数：{m_ReferenceCount}，frame count: {Time.frameCount}, ticks {System.DateTime.Now.Ticks}\nstream：{OpenRead(false)?.Name()}");
            }

            // 如果DisposeImmediately == false，那么Detach并不能真正释放资源，这里手动释放一下
            if (!DisposeImmediately)
            {
                OnSleep();
            }
        }

        /// <summary>
        /// 引用计数为1时触发
        /// awake只处理最新的一个，sleep则批量处理所有
        /// </summary>
        protected virtual void OnAwake()
        {
            UpdateLastEnvironmentTickCount();
        }

        /// <summary>
        /// 引用计数为0时触发
        /// awake只处理最新的一个，sleep则批量处理所有
        /// </summary>
        protected virtual void OnSleep()
        {
            //Debug.Log($"AccessEnvironmentTickCountDuring {Environment.TickCount - m_LastAccessEnvironmentTickCount}");
        }
    }
}
