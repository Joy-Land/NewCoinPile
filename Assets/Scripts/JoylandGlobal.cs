using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Joyland.GamePlay
{
    [Preserve]
    public enum UIViewID
    {
        UINewHome = 100,
    }
    [Preserve]
    public enum UICompID
    {
        UITestComponent = 200,
        UITest1Component = 201,
    }
    [Preserve]
    public static class J
    {
#if UNITY_EDITOR
        public static MiniGameBase Minigame = new WechatMiniGame();
#else
#if UNITY_WX
    public static MiniGameBase Minigame = new WechatMiniGame();
#else
    //TODO Tiktok
    public static MiniGameBase Minigame = new TiktokMiniGame();
#endif
#endif
    }
}

