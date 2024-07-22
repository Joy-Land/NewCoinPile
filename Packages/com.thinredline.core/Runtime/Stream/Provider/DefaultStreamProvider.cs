/*
功能描述：
    本模块用于维护DefaultStreamProvider，其作用是当有stream的时候，直接使用；否则打开一个新的（考虑首次或者中途关闭的情形）
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ThinRL.Core.FileSystem
{
    public class DefaultStreamProvider : StreamProviderBase
    {
        Stream m_Stream;

        public DefaultStreamProvider() : this(default)
        {

        }

        public DefaultStreamProvider(OpenReadParameter parameter) : base(parameter)
        {

        }

        protected override Stream OpenRead(bool tryCreateIfNotExists = true)
        {
            UnityEngine.Profiling.Profiler.BeginSample("StreamProvider.Provide");

            if (m_Stream == null && tryCreateIfNotExists)
            {
                m_Stream = base.OpenRead(tryCreateIfNotExists);
            }

            UnityEngine.Profiling.Profiler.EndSample();

            return m_Stream;
        }

        protected override void OnSleep()
        {
            UnityEngine.Profiling.Profiler.BeginSample("StreamProvider.OnSleep");
            if (m_Stream != null)
            {
                m_Stream.Dispose();
                m_Stream = null;

                base.OnSleep();
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}
