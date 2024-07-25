using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Joyland.GamePlay
{
    [Preserve]
    public static class GamePlayerPrefsKey
    {
        public static readonly string UserCookie = "userCookie";
        public static readonly string AudioSetting = "audioSetting";
        public static readonly string EnableVibrate = "enableVibrate";
        public static readonly string FakeLoginCode = "fakeLoginCode";
    }

    [Preserve]
    public static class GameEventGlobalDefine
    {
        public static readonly string AddProgressBar = "AddProgressBar";
        public static readonly string DownloadFinish = "DownloadFinish";

        public static readonly string EverythingIsReady = "EverythingIsReady";
    }

    [Preserve]
    public enum LoadingStageEventCode
    {
        Init = 0,
        DownloadPackage,
        InitResource,
        Finish
    }

    [Preserve]
    public enum UIViewID
    {
        UINewHome = 100,
        UIStartGame = 101,
        UISetting = 102,
        UIHomePage = 103,
        UIPopupItem = 104,
        UIBank = 105,

    }
    [Preserve]
    public enum UICompID
    {
        UITestComponent = 200,
        UITest1Component = 201,
    }

    [Preserve]
    public class ProtoUserLoginStruct
    {
        public bool isBeginner;
        public string id;
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

