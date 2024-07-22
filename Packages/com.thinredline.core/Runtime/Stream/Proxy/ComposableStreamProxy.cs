/*
功能描述：
    本模块用于维护ComposableStreamProxy，包含多种组件
注意事项：
    1. substream包含自身状态，只能独立持有，不能放入pool进行共享
    
代办事项：
    
主要功能：
    1. converter: 数据转换
    2. provider：可控文件流，可关闭与打开FileStream，保证实际的文件句柄数量不会超过操作系统的限制
    3. subSteam：通过对pkg内子文件的位置做偏移，实现直接通过pkg的文件流，读取子文件内容
*/

using System.IO;

using Better.StreamingAssets;

namespace ThinRL.Core.FileSystem
{
    public class ComposableStreamProxy : StreamProxy
    {
        public IAttachableStreamProvider provider => m_Provider;
        private IAttachableStreamProvider m_Provider;

        //public IStreamConverter converter => m_Converter;
        private IStreamConverter m_Converter;

        private SubReadOnlyStream m_SubStream;

        public ComposableStreamProxy()
        {
        }

        public ComposableStreamProxy(IStreamConverter converter)
        {
            this.m_Converter = converter;
        }

        public ComposableStreamProxy(IAttachableStreamProvider provider) : this(provider, null)
        {

        }

        public ComposableStreamProxy(IAttachableStreamProvider provider, IStreamConverter converter)
        {
            this.m_Provider = provider;
            this.m_Converter = converter;

            var parameter = this.m_Provider.parameter;

            if (parameter.startPosition != 0)
            {
                m_SubStream = new SubReadOnlyStream(null, parameter.startPosition, parameter.fileSize, leaveOpen: false);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            m_Provider?.UpdateLastEnvironmentTickCount();

            return base.Seek(offset, origin);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var positionOld = Position;

            UnityEngine.Profiling.Profiler.BeginSample("Read");
            var length = base.Read(buffer, offset, count);
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("Converter");
            m_Converter?.Convert(buffer, offset, length, positionOld);
            UnityEngine.Profiling.Profiler.EndSample();

            return length;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            m_Converter?.Convert(buffer, offset, count, Position);

            base.Write(buffer, offset, count);
        }

        public override Stream Provide()
        {
            var result = provider?.Provide();

            // 如果是子文件流（better stream assets）
            if (m_SubStream != null)
            {
                m_SubStream.actualStream = result;
                result = m_SubStream;
            }

            return result;
        }

        public override string ToString()
        {
            return provider?.Provide().Name();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Converter?.Dispose();
                m_Converter = null;

                m_Provider?.Dispose();
                m_Provider = null;

                if (m_SubStream != null)
                {
                    // 在detach的过程中，也需要置空...
                    m_SubStream.actualStream = null;

                    m_SubStream?.Dispose();
                    m_SubStream = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
