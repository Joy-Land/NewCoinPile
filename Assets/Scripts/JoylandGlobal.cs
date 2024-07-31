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
        UITips = 106,
        UICollect = 107,
        UICollectInfo = 108,
        UIGameOver = 109,
        UIGameSettlement = 110,
        UIGamePage = 111,
    }
    [Preserve]
    public enum UICompID
    {
        UITestComponent = 200,
        UITest1Component = 201,
        UICollectItemComponent = 202,
    }

    [Preserve]
    public enum ItemCategoryID:int
    {
        Default = 0,
        Share = 1,
        Video = 2,
    }

    [Preserve]
    public enum ItemID
    {

    }

    [Preserve]
    public class ProtoEmptyStruct
    {

    }

    /// <summary>
    /// 登录成功协议返回结构
    /// </summary>
    [Preserve]
    public class ProtoUserLoginStruct
    {
        public bool isBeginner;
        public string id;
    }

    /// <summary>
    /// 道具下种类信息
    /// </summary>
    public class ItemCategoryData
    {
        public int categoryId { get; set; }
        public string categoryName { get; set; }
        public int currentRemainedNumber { get; set; }
    }

    /// <summary>
    /// 道具信息协议返回结构
    /// </summary>
    public class ProtoItemDataStruct
    {
        public int itemId { get; set; }
        public string itemName { get; set; }
        public List<ItemCategoryData> categoryList { get; set; }
    }

    /// <summary>
    /// 道具列表协议返回结构
    /// </summary>
    public class ProtoItemListDataStruct
    {
        public string version { get; set; }
        public List<ProtoItemDataStruct> itemList { get; set; }
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

        public static GameRequestHelper ReqHelper = new GameRequestHelper();

        public static GamePlayerDataManager PlayerDataManager = new GamePlayerDataManager();

    }
}

