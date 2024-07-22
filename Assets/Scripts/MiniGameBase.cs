using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using WeChatWASM;

[Preserve]
public class MinigameGlobalConfig
{
    public struct ShareInfo
    {
        public string title;
        public string imgUrl;
    }

    public struct Share
    {
        public bool isShare;
        public int shareTime;
        public ShareInfo[] shareInfo;
    }
}

public enum JVibrateType
{
    //heavy、medium、light
    Heavy,
    Medium,
    Light
}
[Preserve]
public class JSystemInfoHost
{
    //
    // 摘要:
    //     宿主 app 对应的 appId
    public string appId;
}

[Preserve]
public class JSystemInfo
{
    //
    // 摘要:
    //     需要基础库： `1.1.0` 客户端基础库版本
    public string SDKVersion;

    //
    // 摘要:
    //     需要基础库： `2.6.0` 允许微信使用相册的开关（仅 iOS 有效）
    public bool albumAuthorized;

    public double benchmarkLevel;

    //
    // 摘要:
    //     需要基础库： `2.6.0` 蓝牙的系统开关
    public bool bluetoothEnabled;

    //
    // 摘要:
    //     需要基础库： `1.5.0` 设备品牌
    public string brand;

    //
    // 摘要:
    //     需要基础库： `2.6.0` 允许微信使用摄像头的开关
    public bool cameraAuthorized;

    //
    // 摘要:
    //     设备方向 可选值： - 'portrait': 竖屏; - 'landscape': 横屏;
    public string deviceOrientation;

    //
    // 摘要:
    //     需要基础库： `2.15.0` 是否已打开调试。可通过右上角菜单或 [wx.setEnableDebug](https://developers.weixin.qq.com/minigame/dev/api/base/debug/wx.setEnableDebug.html)
    //     打开调试。
    public bool enableDebug;

    //
    // 摘要:
    //     需要基础库： `1.5.0` 用户字体大小（单位px）。以微信客户端「我-设置-通用-字体大小」中的设置为准
    public double fontSizeSetting;

    //
    // 摘要:
    //     需要基础库： `2.12.3` 当前小程序运行的宿主环境
    public JSystemInfoHost host;

    //
    // 摘要:
    //     微信设置的语言
    public string language;

    //
    // 摘要:
    //     需要基础库： `2.6.0` 允许微信使用定位的开关
    public bool locationAuthorized;

    //
    // 摘要:
    //     需要基础库： `2.6.0` 地理位置的系统开关
    public bool locationEnabled;

    //
    // 摘要:
    //     `true` 表示模糊定位，`false` 表示精确定位，仅 iOS 支持
    public bool locationReducedAccuracy;

    //
    // 摘要:
    //     需要基础库： `2.6.0` 允许微信使用麦克风的开关
    public bool microphoneAuthorized;

    //
    // 摘要:
    //     设备型号。新机型刚推出一段时间会显示unknown，微信会尽快进行适配。
    public string model;

    //
    // 摘要:
    //     需要基础库： `2.6.0` 允许微信通知带有提醒的开关（仅 iOS 有效）
    public bool notificationAlertAuthorized;

    //
    // 摘要:
    //     需要基础库： `2.6.0` 允许微信通知的开关
    public bool notificationAuthorized;

    //
    // 摘要:
    //     需要基础库： `2.6.0` 允许微信通知带有标记的开关（仅 iOS 有效）
    public bool notificationBadgeAuthorized;

    //
    // 摘要:
    //     需要基础库： `2.6.0` 允许微信通知带有声音的开关（仅 iOS 有效）
    public bool notificationSoundAuthorized;

    //
    // 摘要:
    //     需要基础库： `2.19.3` 允许微信使用日历的开关
    public bool phoneCalendarAuthorized;

    //
    // 摘要:
    //     设备像素比
    public double pixelRatio;

    //
    // 摘要:
    //     客户端平台 可选值： - 'ios': iOS微信（包含 iPhone、iPad）; - 'android': Android微信; - 'windows':
    //     Windows微信; - 'mac': macOS微信; - 'devtools': 微信开发者工具;
    public string platform;

    //
    // 摘要:
    //     需要基础库： `2.7.0` 在竖屏正方向下的安全区域。部分机型没有安全区域概念，也不会返回 safeArea 字段，开发者需自行兼容。
    public SafeArea safeArea;

    //
    // 摘要:
    //     需要基础库： `1.1.0` 屏幕高度，单位px
    public double screenHeight;

    //
    // 摘要:
    //     需要基础库： `1.1.0` 屏幕宽度，单位px
    public double screenWidth;

    //
    // 摘要:
    //     需要基础库： `1.9.0` 状态栏的高度，单位px
    public double statusBarHeight;

    //
    // 摘要:
    //     操作系统及版本
    public string system;

    //
    // 摘要:
    //     微信版本号
    public string version;

    //
    // 摘要:
    //     需要基础库： `2.6.0` Wi-Fi 的系统开关
    public bool wifiEnabled;

    //
    // 摘要:
    //     可使用窗口高度，单位px
    public double windowHeight;

    //
    // 摘要:
    //     可使用窗口宽度，单位px
    public double windowWidth;

    //
    // 摘要:
    //     需要基础库： `2.11.0` 系统当前主题，取值为`light`或`dark`，全局配置`"darkmode":true`时才能获取，否则为 undefined
    //     （不支持小游戏） 可选值： - 'dark': 深色主题; - 'light': 浅色主题;
    public string theme;
}


[Preserve]
public class JResultReferrerInfo
{
    //
    // 摘要:
    //     来源小程序或公众号或App的 appId
    public string appId;

    //
    // 摘要:
    //     来源小程序传过来的数据，scene=1037或1038时支持
    public Dictionary<string, string> extraData;
}
public class JOnShowResultParams
{
    //
    // 摘要:
    //     查询参数
    public Dictionary<string, string> query;

    //
    // 摘要:
    //     当场景为由从另一个小程序或公众号或App打开时，返回此字段
    public JResultReferrerInfo referrerInfo;

    //
    // 摘要:
    //     场景值
    public double scene;

    //
    // 摘要:
    //     从微信群聊/单聊打开小程序时，chatType 表示具体微信群聊/单聊类型 可选值： - 1: 微信联系人单聊; - 2: 企业微信联系人单聊; - 3:
    //     普通微信群聊; - 4: 企业微信互通群聊;
    public double? chatType;

    //
    // 摘要:
    //     shareTicket
    public string shareTicket;
}

[Preserve]
public class JTouch
{
    //
    // 摘要:
    //     触点相对于可见视区左边沿的 X 坐标。
    public float clientX;

    //
    // 摘要:
    //     触点相对于可见视区上边沿的 Y 坐标。
    public float clientY;

    //
    // 摘要:
    //     手指挤压触摸平面的压力大小, 从0.0(没有压力)到1.0(最大压力)的浮点数（仅在支持 force touch 的设备返回）
    public double force;

    //
    // 摘要:
    //     Touch 对象的唯一标识符，只读属性。一次触摸动作(我们值的是手指的触摸)在平面上移动的整个过程中, 该标识符不变。可以根据它来判断跟踪的是否是同一次触摸过程。
    public int identifier;

    //
    // 摘要:
    //     触点相对于页面左边沿的 X 坐标。
    public float pageX;

    //
    // 摘要:
    //     触点相对于页面上边沿的 Y 坐标。
    public float pageY;
}
public class JOnTouchStartListenerResult
{
    //
    // 摘要:
    //     触发此次事件的触摸点列表
    public JTouch[] changedTouches;

    //
    // 摘要:
    //     事件触发时的时间戳
    public long timeStamp;

    //
    // 摘要:
    //     当前所有触摸点的列表
    public JTouch[] touches;
}

public enum JPlantform
{
    Editor,
    Wechat,
    Tiktok
}

public class MiniGameBase
{

    protected JSystemInfo m_SystemInfo = new JSystemInfo();
    public JSystemInfo SystemInfo
    {
        get { return m_SystemInfo; }
        protected set { m_SystemInfo = value; }
    }

    protected bool m_EnableVibrate;
    public bool EnableVibrate
    {
        get { return m_EnableVibrate; }
        protected set { m_EnableVibrate = value; }
    }

    protected JPlantform m_CurrentPlatform = JPlantform.Editor;
    public JPlantform CurrentPlatform
    {
        get { return m_CurrentPlatform; }
        protected set { m_CurrentPlatform = value; }
    }

    public Action<JOnShowResultParams> OnGameShowGlobalEvent;
    public Action OnGameHideGlobalEvent;

    public Action<JOnTouchStartListenerResult> OnTouchStartGlobalEvent;
    public Action<JOnTouchStartListenerResult> OnTouchMoveGlobalEvent;
    public Action<JOnTouchStartListenerResult> OnTouchEndGlobalEvent;

    protected Action m_ShareSuccessRewardCallback;


    public static void CreateUserInfoButtonStyle(int width, int height, bool debug)
    {
        var bgColor = "#00000000";
        var color = "#ffffff";
        var borderColor = "#00000000";

        if (debug)
        {
            bgColor = "#ff0000";
            color = "#ffffff";
            borderColor = "#ff0000";
        }

    }

    /// <summary>
    /// 在最早的时候进行初始化操作（最起码要在网络前）
    /// </summary>
    /// <param name="enableVibrate"></param>
    /// <param name="enableShareMenu"></param>
    public virtual void Initialize(bool enableVibrate, bool enableShareMenu)
    {
        // UNITY_WX
    }

    public virtual void UpdateSystemInfo()
    {

    }

    public virtual void CreateUserInfoButton()
    {

    }

    public virtual void UserLogin()
    {

    }

    public virtual void Share(Action reward = null)
    {

    }

    /// <summary>
    /// 交给我小博哥写吧，视频这块不太熟
    /// </summary>
    /// <param name="reward"></param>
    /// <exception cref="NotImplementedException"></exception>
    public virtual void Video(Action reward = null)
    {
        throw new NotImplementedException();
    }


    public virtual void VibrateShort(JVibrateType type, Action successCB = null)
    {
        //heavy、medium、light

    }

    public virtual void VibrateLong(Action successCB = null)
    {

    }

    public virtual void ShowToast(string title, string icon = "none", float duration = 1, Action successCB = null)
    {

    }

    public virtual void ShowLoading(string title)
    {

    }

    public virtual void HideLoading()
    {

    }


    protected void OnGameShow(JOnShowResultParams res)
    {
        OnGameShowGlobalEvent?.Invoke(res);
    }

    protected void OnGameHide()
    {
        OnGameHideGlobalEvent?.Invoke();
    }

    protected void OnTouchStart(JOnTouchStartListenerResult res)
    {
        OnTouchStartGlobalEvent?.Invoke(res);
    }

    protected void OnTouchMove(JOnTouchStartListenerResult res)
    {
        OnTouchMoveGlobalEvent?.Invoke(res);
    }

    protected void OnTouchEnd(JOnTouchStartListenerResult res)
    {
        OnTouchEndGlobalEvent?.Invoke(res);
    }

}
