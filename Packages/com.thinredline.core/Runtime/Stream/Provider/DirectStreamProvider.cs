/*
功能描述：
    本模块用于维护DirectStreamProvider，直接持有一个stream，而不是stream provider
注意事项：
    1. 独立维护计数，不走基类逻辑
    2. 不提供stream重连功能，如果需要，重新new一个provider
*/


using System.IO;

namespace ThinRL.Core.FileSystem
{
    public class DirectStreamProvider : StreamProviderBase
    {
        Stream m_Stream;

        public DirectStreamProvider() : base(default)
        {

        }

        public DirectStreamProvider(Stream stream) : base(default)
        {
            m_Stream = stream;

            if (m_Stream != null)
            {
                IncreaseReferenceCount();

                OnAwake();
            }
        }

        protected override Stream OpenRead(bool tryCreateIfNotExists = true)
        {
            return m_Stream;
        }

        // 覆盖
        public override void Attach()
        {
            IncreaseReferenceCount();
        }

        public override void Detach()
        {
            DecreaseReferenceCount();
        }

        protected override void OnSleep()
        {
            m_Stream?.Dispose();
            m_Stream = null;

            base.OnSleep();
        }
    }
}
