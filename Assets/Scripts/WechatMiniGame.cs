using Cysharp.Threading.Tasks;
using Joyland;
using Joyland.GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using ThinRL.Core.Tools;
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

    public override void GetUserInfo(Action<JGetUserInfoSuccessCallbackResult> successCb, Action failCb = null)
    {
        base.GetUserInfo(successCb, failCb);
        if (!CheckWXValid())
        {
            successCb?.Invoke(null);
            return;
        }
        WX.GetUserInfo(new GetUserInfoOption()
        {
            success = (res) =>
            {
                JGetUserInfoSuccessCallbackResult result = new JGetUserInfoSuccessCallbackResult();
                result.cloudID = res.cloudID;
                result.encryptedData = res.encryptedData;
                result.iv = res.iv;
                result.rawData = res.rawData;
                result.signature = res.signature;
                result.errMsg = res.errMsg;
                result.userInfo = new JUserInfo();
                result.userInfo.nickName = res.userInfo.nickName;
                result.userInfo.avatarUrl = res.userInfo.avatarUrl;
                result.userInfo.city = res.userInfo.city;
                result.userInfo.country = res.userInfo.country;
                result.userInfo.gender = res.userInfo.gender;
                result.userInfo.language = res.userInfo.language;
                result.userInfo.province = res.userInfo.province;
                successCb?.Invoke(result);
            },
            fail = (res) =>
            {
                console.error("无法获取到UserInfo", res.errMsg);
            }
        });
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
        WX.ShowToast(new ShowToastOption() { title = title, duration = duration, icon = icon, mask = false, success = (e)=> { successCB?.Invoke(); } });
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
                m_AuthData.hasUserInfoAuth = res.authSetting["scope.userInfo"];
                m_AuthData.hasFuzzyLocationAuth = res.authSetting["scope.userFuzzyLocation"];
                m_AuthData.hasWeRunAuth = res.authSetting["scope.werun'"];
                m_AuthData.hasWritePhtosAlbum = res.authSetting["scope.writePhotosAlbum'"];
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
            m_SystemInfo.statusBarHeight = info.statusBarHeight;
            m_SystemInfo.host = new JSystemInfoHost() { appId = info.host.appId };
            m_SystemInfo.enableDebug = info.enableDebug;
            m_SystemInfo.safeArea = new JSafeArea();
            m_SystemInfo.safeArea.top = info.safeArea.top;
            m_SystemInfo.safeArea.right = info.safeArea.right;
            m_SystemInfo.safeArea.left = info.safeArea.left;
            m_SystemInfo.safeArea.bottom = info.safeArea.bottom;
            m_SystemInfo.safeArea.width = info.safeArea.width;
            m_SystemInfo.safeArea.height = info.safeArea.height;
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

    public override void UserLogin(Action<LoginSuccessReturnData> successCb, Action failCb = null)
    {
        base.UserLogin(successCb, failCb);

        if (!CheckWXValid())
        {
            return;
        }

        Action loginAgain = delegate ()
        {
            Func<UniTaskVoid> retry = async delegate ()
            {
                await UniTask.Delay(TimeSpan.FromMilliseconds(1000));
                if(m_CurrentLoginCount >= MAX_RETRY_LOGIN_COUNT)
                {
                    failCb?.Invoke();
                }
                else
                {
                    UserLogin(successCb, failCb);
                    ShowToast("正在尝试重新登录");
                    console.error("login_fail: ====>", m_CurrentLoginCount);
                    m_CurrentLoginCount += 1;
                }
            };

            retry.Invoke();
        };

        if(CheckWXValid())
        {
            WX.Login(new LoginOption()
            {
                success = (e) =>
                {
                    var data = new { code = e.code };
                    console.info(data);
                    NetworkManager.Instance.GetReq<ProtoUserLoginStruct>("login", data, (res) =>
                    {
                        if (res.isSuccessed)
                        {
                            if (res.resData.code == 0)
                            {
                                if (res.headers.TryGetValue("Set-Cookie", out var cookieString))
                                {
                                    NetworkManager.Instance.CacheCookieSync(cookieString);
                                }
                                else
                                {
                                    console.error("【重要警告】！！！！！！！！！！无cookie！！！！！！");
                                }

                                ProtoUserLoginStruct userLoginResultData = new ProtoUserLoginStruct()
                                {
                                    id = res.resData.data.id,
                                    isBeginner = res.resData.data.isBeginner
                                };
                                //已经获得用户授权的话，直接发
                                if (m_AuthData.hasUserInfoAuth)
                                {
                                    GetUserInfo((userInfo) =>
                                    {
                                        successCb?.Invoke(new LoginSuccessReturnData() { info = userInfo, userLoginReturnData = userLoginResultData });
                                    });
                                }
                                else
                                {
                                    successCb?.Invoke(new LoginSuccessReturnData() { info = null, userLoginReturnData = userLoginResultData });
                                }
                            }
                            else
                            {
                                console.error("error code:", res.resData.code);
                                loginAgain();
                            }
                        }
                        else
                        {
                            loginAgain();
                        }

                    });
                },
                fail = (e) =>
                {
                    loginAgain();
                }
            });
        }
        else
        {
            var fakeCode = PlayerPrefsManager.GetString(GamePlayerPrefsKey.FakeLoginCode, "");
            if(fakeCode == "")
            {
                fakeCode = CommonUtil.Random.GenerateRandomCodeWithTimestamp();
                PlayerPrefsManager.SetString(GamePlayerPrefsKey.FakeLoginCode, fakeCode);
            }
            var data = new { code = fakeCode };
            console.info(data);
            NetworkManager.Instance.GetReq<ProtoUserLoginStruct>("fake_login", data, (res) =>
            {
                if (res.isSuccessed)
                {
                    if (res.resData.code == 0)
                    {
                        if (res.headers.TryGetValue("Set-Cookie", out var cookieString))
                        {
                            NetworkManager.Instance.CacheCookieSync(cookieString);
                        }
                        else
                        {
                            console.error("【重要警告】！！！！！！！！！！无cookie！！！！！！");
                        }

                        ProtoUserLoginStruct userLoginResultData = new ProtoUserLoginStruct()
                        {
                            id = res.resData.data.id,
                            isBeginner = res.resData.data.isBeginner
                        };
                        successCb?.Invoke(new LoginSuccessReturnData() { info = null, userLoginReturnData = userLoginResultData });
                    }
                    else
                    {
                        console.error("error code:", res.resData.code);
                        loginAgain();
                    }
                }
                else
                {
                    loginAgain();
                }

            });
        }

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
