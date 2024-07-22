/*
功能描述：
    本模块用于维护ThreadBindStreamProvider，思路是给每个线程一个stream，这样各个stream之间不受干扰
    默认采用索引数组映射stream缓存，原理是创建跟线程池线程数量等长的数组(1000左右)，根据线程id直接从数组中拿对应的stream，但
考虑线程池数量可能改变，此时需要重新创建该索引数组，如果有稳定性问题，可给模块定义UseDictionaryStreamPool宏，开启Dictionary
缓存方式（高并发的调用Dictionary.TryGetValue的话，效率有点影响但不严重）
    
    ThreadBindStreamProvider与file_name的数量关系为：m:n
    ThreadBindStreamProviderUtility维护了file_name与ThreadBindStreamEntry的映射，数量关系为：1:1
    ThreadBindStreamEntry维护file_name对应的stream线程绑定池，映射关系：1:io_thread_count
    
注意事项：
    1. 调用频率非常高，非调试原因不要打日志
    2. 根据现在的调试信息，会有20个左右的线程(并行情况待测试)
    3. Dictionary也可以考虑ConcurrentDictionary
    4. ThreadBindStreamEntry应当由主线程创建，子线程只管拿已有的即可，但Entry内部需要线程安全，因为可能从不同子线程发起
*/

#if UNITY_EDITOR
//#define DebugThreadParallelism
#endif


using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThinRL.Core.FileSystem
{
    public class ThreadBindStreamProvider : StreamProviderBase
    {
        ThreadBindStreamEntry m_ThreadBindStreamEntry;

        public ThreadBindStreamProvider() : this(default)
        {
        }

        public ThreadBindStreamProvider(OpenReadParameter parameter) : base(parameter)
        {
            // main thread
            m_ThreadBindStreamEntry = ThreadBindStreamProviderUtility.GetOrCreateThreadBindStreamEntry(this);
        }

        protected override Stream OpenRead(bool tryCreateIfNotExists = true)
        {
            // main thread and sub thread
            return m_ThreadBindStreamEntry?.GetOrCreateStream(tryCreateIfNotExists ? this : null);
        }

        protected override void OnAwake()
        {
            // main thread
            m_ThreadBindStreamEntry.AddReference(this);
        }

        protected override void OnSleep()
        {
            // main thread
            m_ThreadBindStreamEntry.RemoveReference(this);

            base.OnSleep();
        }
    }

    internal class ThreadBindStreamProviderUtility
    {
        /// <summary>
        /// file:stream_entry
        /// </summary>
        static readonly Dictionary<string, ThreadBindStreamEntry> s_ThreadBindStreamEntryPool = new Dictionary<string, ThreadBindStreamEntry>();

        public static ThreadBindStreamEntry GetOrCreateThreadBindStreamEntry(IAttachableStreamProvider provider)
        {
            ThreadBindStreamEntry result = null;

            if (provider != null && !s_ThreadBindStreamEntryPool.TryGetValue(provider.parameter.filePath, out result))
            {
                result = new ThreadBindStreamEntry();
                s_ThreadBindStreamEntryPool.Add(provider.parameter.filePath, result);

                // main thread
                Debug.Assert(System.Threading.Thread.CurrentThread.ManagedThreadId == 1);
            }

            return result;
        }
    }

    internal class ThreadBindStreamEntry
    {
        readonly object k_Locker = new object();

        /// <summary>
        /// 线程池大小，用来初始化数组版缓存，并基于此做散列查询，理论上可以将ManagedThreadId直接散列到与ThreadPoolSize等长的数组上
        /// </summary>
        int m_ThreadPoolSize;

        /// <summary>
        /// 缓存：数组版，长度大约1000，实际有效20个左右
        /// </summary>
        Stream[] m_StreamArray;

        /// <summary>
        /// 缓存：字典版本
        /// </summary>
        readonly Dictionary<int, Stream> k_StreamDictionary = new Dictionary<int, Stream>();

        /// <summary>
        /// 引用该StreamEntry的所有Provider集合，当集合清空时，释放该Entry打开的所有Stream资源
        /// </summary>
        readonly HashSet<IAttachableStreamProvider> k_References = new HashSet<IAttachableStreamProvider>();

#if UseDictionaryStreamPool
        internal static readonly bool s_UseDictionaryStreamPool = true;
#else
        internal static readonly bool s_UseDictionaryStreamPool = false;
#endif

        public ThreadBindStreamEntry()
        {
            if (!s_UseDictionaryStreamPool)
            {
                TryUpdateStreamCache_ArrayVersion();
            }
        }

        public void AddReference(IAttachableStreamProvider provider)
        {
            k_References.Add(provider);
        }

        public void RemoveReference(IAttachableStreamProvider provider)
        {
            k_References.Remove(provider);

            if (k_References.Count == 0)
            {
                Clear();
            }
        }

        public Stream GetOrCreateStream(IAttachableStreamProvider provider)
        {
            if (!s_UseDictionaryStreamPool)
            {
                return Provide_ArrayVersion(provider);
            }
            else
            {
                return Provide_DictionaryVersion(provider);
            }
        }

        Stream Provide_ArrayVersion(IAttachableStreamProvider provider)
        {
            int currentManagedThreadId = Thread.CurrentThread.ManagedThreadId;

            currentManagedThreadId %= m_ThreadPoolSize;

            var cache = m_StreamArray ?? TryUpdateStreamCache_ArrayVersion();

            if (cache[currentManagedThreadId] == null && provider != null)
            {
                lock (k_Locker)
                {
                    if (cache[currentManagedThreadId] == null)
                    {
#if DebugThreadParallelism
                        SampleParallelismInfo();
#endif
                        var result = provider?.delegateOpenRead(provider.parameter);

                        cache[currentManagedThreadId] = result;
                    }
                }
            }

            return cache[currentManagedThreadId];
        }

        Stream Provide_DictionaryVersion(IAttachableStreamProvider provider)
        {
            int currentManagedThreadId = Thread.CurrentThread.ManagedThreadId;

            if (!k_StreamDictionary.TryGetValue(currentManagedThreadId, out var result) && provider != null)
            {
                lock (k_Locker)
                {
                    if (!k_StreamDictionary.TryGetValue(currentManagedThreadId, out result))
                    {
#if DebugThreadParallelism
                        SampleParallelismInfo();
#endif

                        result = provider?.delegateOpenRead(provider.parameter);

                        k_StreamDictionary.Add(currentManagedThreadId, result);
                    }
                }
            }

            return result;
        }

        // 更新ArrayVersion缓存（字典的缓存直接扩展，不进行更新）
        private Stream[] TryUpdateStreamCache_ArrayVersion()
        {
            ThreadPool.GetMaxThreads(out var workerThreads, out var completionPortThreads);

            if (null == m_StreamArray)
            {
                lock (k_Locker)
                {
                    m_ThreadPoolSize = workerThreads;

                    if (null == m_StreamArray)
                    {
                        m_StreamArray = new Stream[m_ThreadPoolSize + 1];
                    }
                }
            }
            else if (m_ThreadPoolSize != workerThreads)
            {
                // 想不出什么时候会发生线程池大小改变，留一条日志监测
                Debug.LogError("线程池大小改变");
            }

            return m_StreamArray;
        }

        void Clear()
        {
            if (!s_UseDictionaryStreamPool)
            {
                Clear_ArrayVersion();
            }
            else
            {
                Clear_DictionaryVersion();
            }

#if DebugThreadParallelism
            ClearDebugThreadParallelismInfo();
#endif
        }

        void Clear_ArrayVersion()
        {
            lock (k_Locker)
            {
                var cache = m_StreamArray;

                m_StreamArray = null;

                if (cache != null)
                {
                    foreach (var stream in cache)
                    {
                        stream?.Dispose();
                    }
                }
            }
        }

        void Clear_DictionaryVersion()
        {
            lock (k_Locker)
            {
                foreach (var stream in k_StreamDictionary.Values)
                {
                    stream?.Dispose();
                }

                k_StreamDictionary.Clear();
            }
        }

#if DebugThreadParallelism
        Dictionary<int, List<double>> m_ParallelismInfo = new Dictionary<int, List<double>>();

        string m_ParallelismInfoFilePath = "Log/ParallelismInfo.txt";

        void SampleParallelismInfo()
        {
            if (!m_ParallelismInfo.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var times))
            {
                times = new List<double>();

                m_ParallelismInfo.Add(Thread.CurrentThread.ManagedThreadId, times);
            }

            times.Add(DateTime.Now.ToOADate());
        }

        public void ClearDebugThreadParallelismInfo()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var kv in m_ParallelismInfo)
            {
                foreach (var time in kv.Value)
                {
                    sb.Append(kv.Key).Append("\t").AppendLine(time.ToString());
                }
            }

            File.WriteAllText(m_ParallelismInfoFilePath, sb.ToString());
        }
#endif  // DebugThreadParallelism
    }
}
