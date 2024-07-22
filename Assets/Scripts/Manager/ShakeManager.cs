using Framework;
using WeChatWASM;

namespace Manager
{
    public class ShakeManager : SingletonBaseMono<ShakeManager>
    {
        public void Vibrate()
        {
            var param = new VibrateShortOption();
            param.type = "medium";
            WX.VibrateShort(param);
        }
    }
}
