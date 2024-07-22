/*
功能描述：
    本模块用于维护StreamConverter，可以当作基类，主要是进行了资源释放的维护，不用每个子类都去维护资源释放
*/

using System;

namespace ThinRL.Core.FileSystem
{
    public class StreamConverter : IStreamConverter
    {
        public virtual void Convert(byte[] buffer, int offset, int length, long position)
        {

        }

        protected virtual void Dispose(bool disposing)
        {

        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
    }
}
