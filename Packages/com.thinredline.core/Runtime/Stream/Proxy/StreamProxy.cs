/*
功能描述：
    本模块用于维护StreamProxy，只是作为基类
*/

using System.IO;

namespace ThinRL.Core.FileSystem
{
    public class StreamProxy : Stream
    {
        private Stream m_Source => Provide();

        public override bool CanRead => m_Source.CanRead;

        public override bool CanSeek => m_Source.CanSeek;

        public override bool CanWrite => m_Source.CanWrite;

        public override long Length => m_Source.Length;

        public override long Position { get => m_Source.Position; set => m_Source.Position = value; }

        public override void Flush() => m_Source.Flush();

        public override long Seek(long offset, SeekOrigin origin) => m_Source.Seek(offset, origin);

        public override void SetLength(long value) => m_Source.SetLength(value);

        public StreamProxy()
        {
        }

        public static int MillisecondsSleepForDebug = 0;

        public override int Read(byte[] buffer, int offset, int count)
        {
            var length = m_Source.Read(buffer, offset, count);

            if (MillisecondsSleepForDebug > 0 && System.Threading.Thread.CurrentThread.ManagedThreadId != 1)
            {
                System.Threading.Thread.Sleep(MillisecondsSleepForDebug);
            }

            return length;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            m_Source.Write(buffer, offset, count);
        }

        /// <summary>
        /// k_Source与provider互斥，且有限使用k_Source
        /// </summary>
        /// <returns></returns>
        public virtual Stream Provide()
        {
            return null;
        }
    }
}
