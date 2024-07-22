/*
功能描述：
    本模块用于维护可计数的StreamProxy，需直接包装FileStream，否则无法精准计数
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ThinRL.Core.FileSystem
{
    public class CountableStreamProxy : StreamProxy
    {
        // 回调，方便上层进行数据统计
        // string: file
        // Stream: stream or null
        // bool:   construct or destruct
        public static Action<string, Stream, bool> OnStreamProxyLifeCycle { get; set; }

        // 实际打开的stream数量
        public static int FileStreamCount => s_FileStreamCount;
        static int s_FileStreamCount;

        Stream m_Source;

        // 缓存一下，方便查看是哪些stream
        // 同一个文件也可能打开多个流，所以不要用hash族进行统计
        static List<string> s_StreamNames { get; } = new List<string>();

        public static bool EnableRecordStreamNames => false;

        public CountableStreamProxy() : this(null)
        {
        }

        public CountableStreamProxy(Stream stream)
        {
            m_Source = stream;

            OnStreamProxyLifeCycle?.Invoke(m_Source.Name(), m_Source, true);

            if (EnableRecordStreamNames)
            {
                s_StreamNames.Add(m_Source.Name());
            }

            Interlocked.Increment(ref s_FileStreamCount);
        }

        public override Stream Provide()
        {
            return m_Source;
        }

        public override string ToString()
        {
            return m_Source?.Name();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && m_Source != null)
            {
                Interlocked.Decrement(ref s_FileStreamCount);

                if (EnableRecordStreamNames)
                {
                    s_StreamNames.Remove(m_Source.Name());
                }

                OnStreamProxyLifeCycle?.Invoke(m_Source.Name(), m_Source, false);

                m_Source?.Dispose();
                m_Source = null;
            }

            base.Dispose(disposing);
        }
    }
}
