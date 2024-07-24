using System;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.Scripting;
using WeChatWASM;

[Preserve]
public class WechatMiniGame : MiniGameBase
{

    private float m_BeforeShareTime = 0;
    private bool m_StartShare = false;
    public static bool CheckWXValid()
    {
#if UNITY_WX && !UNITY_EDITOR
            return true;
#endif
        return false;
    }

    public override void Initialize(bool enableVibrate, bool enableShareMenu)
    {
        m_EnableVibrate = enableVibrate;
        if (!CheckWXValid())
        {
            return;
        }
        m_CurrentPlatform = JPlantform.Wechat;
        WX.InitSDK((code) =>
        {
            EnableShareMenu(enableShareMenu);

            UpdateWxSettings();
            UpdateSystemInfo();

            var fallbackFont = Application.streamingAssetsPath + "fallback.ttf";
            WX.GetWXFont(fallbackFont, (font) =>
            {
                m_SystemFont = font;
            });

            WX.OnShow((res) =>
            {
                JOnShowResultParams p = new JOnShowResultParams();
                p.shareTicket = res.shareTicket;
                p.referrerInfo = new JResultReferrerInfo() { appId = res.referrerInfo.appId, extraData = res.referrerInfo.extraData };
                p.query = res.query;
                p.scene = res.scene;
                p.chatType = res.chatType;

                OnGameShow(p);

                if (m_StartShare)
                {
                    ShareCallback(() =>
                    {
                        m_StartShare = false;
                        m_ShareSuccessRewardCallback?.Invoke();
                    }, () =>
                    {
                        m_StartShare = false;
                    });
                }
            });

            var p = GetShareAppMessageParam();
            WX.OnShareAppMessage(p, (e) =>
            {
                e?.Invoke(p);
            });

            WX.OnHide((res) =>
            {
                OnGameHide();
            });

            WX.OnTouchStart((res) =>
            {
                OnTouchStart(touchResConvert(res));
            });

            WX.OnTouchMove((res) =>
            {
                OnTouchMove(touchResConvert(res));
            });

            WX.OnTouchEnd((res) =>
            {
                OnTouchEnd(touchResConvert(res));
            });
        });
    }

    private void EnableShareMenu(bool show)
    {
        if (!CheckWXValid())
        {
            return;
        }
        if (show)
        {
            WX.ShowShareMenu(new ShowShareMenuOption() { menus = new string[] { "shareAppMessage", "shareTimeline" }, withShareTicket = true });
        }
        else
        {
            WX.HideShareMenu(new HideShareMenuOption() { menus = new string[] { "shareAppMessage", "shareTimeline" } });
        }
    }

    private JOnTouchStartListenerResult touchResConvert(OnTouchStartListenerResult res)
    {
        JOnTouchStartListenerResult p = new JOnTouchStartListenerResult();
        p.changedTouches = new JTouch[res.changedTouches.Length];
        p.touches = new JTouch[res.touches.Length];
        p.timeStamp = res.timeStamp;
        for (int i = 0; i < res.changedTouches.Length; i++)
        {
            p.changedTouches[i] = new JTouch()
            {
                clientX = res.changedTouches[i].clientX,
                clientY = res.changedTouches[i].clientY,
                force = res.changedTouches[i].force,
                pageX = res.changedTouches[i].pageX,
                pageY = res.changedTouches[i].pageY,
                identifier = res.changedTouches[i].identifier,
            };
        }
        for (int i = 0; i < res.touches.Length; i++)
        {
            p.touches[i] = new JTouch()
            {
                clientX = res.touches[i].clientX,
                clientY = res.touches[i].clientY,
                force = res.touches[i].force,
                pageX = res.touches[i].pageX,
                pageY = res.touches[i].pageY,
                identifier = res.touches[i].identifier,
            };
        }
        return p;
    }

    public override void CreateUserInfoButton()
    {

        if (!CheckWXValid())
        {
            return;
        }

        //WX.CreateUserInfoButton(0, 0,)
    }

    public override void VibrateShort(JVibrateType type, Action successCB = null)
    {
        base.VibrateShort(type, successCB);
        if (!CheckWXValid() || m_EnableVibrate)
        {
            return;
        }
        string vibrateType = "";
        if (type == JVibrateType.Light)
        {
            vibrateType = "light";
        }
        else if (type == JVibrateType.Medium)
        {
            vibrateType = "medium";
        }
        else if (type == JVibrateType.Heavy)
        {
            vibrateType = "heavy";
        }

        var op = new VibrateShortOption()
        {
            type = vibrateType,
            success = (e) =>
            {
                successCB?.Invoke();
            }
        };
        WX.VibrateShort(op);
    }

    public override void VibrateLong(Action successCB = null)
    {
        base.VibrateLong(successCB);
        if (!CheckWXValid() || m_EnableVibrate == false)
        {
            return;
        }
        WX.VibrateLong(new VibrateLongOption()
        {
            success = (e) =>
            {
                successCB?.Invoke();
            }
        });
    }

    public override void ShowLoading(string title)
    {
        base.ShowLoading(title);
        if (!CheckWXValid())
        {
            return;
        }
        WX.ShowLoading(new ShowLoadingOption() { title = title, mask = true });
    }

    public override void HideLoading()
    {
        base.HideLoading();
        if (!CheckWXValid())
        {
            return;
        }
        WX.HideLoading(new HideLoadingOption() { });
    }

    public override void ShowToast(string title, string icon = "none", float duration = 1, Action successCB = null)
    {
        base.ShowToast(title, icon, duration, successCB);
        if (!CheckWXValid())
        {
            return;
        }
        WX.ShowToast(new ShowToastOption() { title = title, duration = duration, icon = icon, mask = true, success = (e)=> { successCB?.Invoke(); } });
    }


    private void ShareCallback(Action successCallback, Action failedCallback)
    {
        successCallback?.Invoke();
    }

    public void UpdateWxSettings()
    {
        if (!CheckWXValid())
        {
            return;
        }

        WX.GetSetting(new GetSettingOption()
        {
            success = (res) =>
            {
                var hasUserInfoAuth = res.authSetting["scope.userInfo"];
                var hasFuzzyLocationAuth = res.authSetting["scope.userFuzzyLocation"];
                var hasWeRunAuth = res.authSetting["scope.werun'"];
                var hasWritePhtosAlbum = res.authSetting["scope.writePhotosAlbum'"];
            }
        });
    }

    public override void UpdateSystemInfo()
    {
        if (!CheckWXValid())
        {
            return;
        }

        var info = WX.GetSystemInfoSync();
        if (info != null)
        {
            m_SystemInfo.brand = info.brand;
            m_SystemInfo.model = info.model;
            m_SystemInfo.pixelRatio = info.pixelRatio;
            m_SystemInfo.screenWidth = info.screenWidth;
            m_SystemInfo.screenHeight = info.screenHeight;
            m_SystemInfo.windowWidth = info.windowWidth;
            m_SystemInfo.windowHeight = info.windowHeight;
            m_SystemInfo.language = info.language;
            m_SystemInfo.version = info.version;
            m_SystemInfo.system = info.system;
            m_SystemInfo.platform = info.platform;
            m_SystemInfo.fontSizeSetting = info.fontSizeSetting;
            m_SystemInfo.SDKVersion = info.SDKVersion;
            m_SystemInfo.host = new JSystemInfoHost() { appId = info.host.appId };
            m_SystemInfo.enableDebug = info.enableDebug;
        }
    }

    public override void Share(Action reward = null)
    {
        if (!CheckWXValid())
        {
            reward?.Invoke();
            return;
        }

        m_BeforeShareTime = CommonUtil.Common.GetNowTime().Millisecond;
        if (reward != null)
        {
            m_StartShare = true;
            m_ShareSuccessRewardCallback = reward;
        }
        else
        {
            m_StartShare = false;
            m_ShareSuccessRewardCallback = () => { };
        }

        WX.ShareAppMessage(GetShareInfo());
    }

    private ShareAppMessageOption GetShareInfo()
    {
        var op = new ShareAppMessageOption();
        op.title = "分享咯";
        op.imageUrl = "";
        op.query = "";
        return op;
    }

    private WXShareAppMessageParam GetShareAppMessageParam()
    {
        var shareInfo = GetShareInfo();
        WXShareAppMessageParam p = new WXShareAppMessageParam();
        p.toCurrentGroup = shareInfo.toCurrentGroup.Value;
        p.query = shareInfo.query;
        p.path = shareInfo.path;
        p.imageUrlId = shareInfo.imageUrlId;
        p.imageUrl = shareInfo.imageUrl;
        p.title = shareInfo.title;
        return p;
    }
}
