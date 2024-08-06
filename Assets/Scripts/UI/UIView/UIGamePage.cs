using Cysharp.Threading.Tasks;
using Joyland.GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

public class UIGamePage : UIViewBase
{
    public Button Btn_Setting;
    public Button Btn_Bank;
    public Button Btn_AddBox;
    public Image Img_AddBoxCount;
    public Text Txt_AddBoxCount;
    public Image Img_AddBoxEmpty;
    public Button Btn_Clear;
    public Image Img_ClearCount;
    public Text Txt_ClearCount;
    public Image Img_ClearEmpty;
    public Button Btn_Magnet;
    public Image Img_MagnetCount;
    public Text Txt_MagnetCount;
    public Image Img_MagnetEmpty;
    public InputField input;
    public Button Btn_xuanguan;




    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);
        Btn_Setting = transform.Find("Btn_Setting").GetComponent<Button>();
        Btn_Bank = transform.Find("Btn_Bank").GetComponent<Button>();
        Btn_AddBox = transform.Find("Btn_AddBox").GetComponent<Button>();
        Img_AddBoxCount = transform.Find("Btn_AddBox/Img_AddBoxCount").GetComponent<Image>();
        Txt_AddBoxCount = transform.Find("Btn_AddBox/Img_AddBoxCount/Txt_AddBoxCount").GetComponent<Text>();
        Img_AddBoxEmpty = transform.Find("Btn_AddBox/Img_AddBoxCount/Img_AddBoxEmpty").GetComponent<Image>();
        Btn_Clear = transform.Find("Btn_Clear").GetComponent<Button>();
        Img_ClearCount = transform.Find("Btn_Clear/Img_ClearCount").GetComponent<Image>();
        Txt_ClearCount = transform.Find("Btn_Clear/Img_ClearCount/Txt_ClearCount").GetComponent<Text>();
        Img_ClearEmpty = transform.Find("Btn_Clear/Img_ClearCount/Img_ClearEmpty").GetComponent<Image>();
        Btn_Magnet = transform.Find("Btn_Magnet").GetComponent<Button>();
        Img_MagnetCount = transform.Find("Btn_Magnet/Img_MagnetCount").GetComponent<Image>();
        Txt_MagnetCount = transform.Find("Btn_Magnet/Img_MagnetCount/Txt_MagnetCount").GetComponent<Text>();
        Img_MagnetEmpty = transform.Find("Btn_Magnet/Img_MagnetCount/Img_MagnetEmpty").GetComponent<Image>();

        Txt_MagnetCount = transform.Find("Btn_Magnet/Image/Txt_MagnetCount").GetComponent<Text>();
        Img_MagnetEmpty = transform.Find("Btn_Magnet/Image/Img_MagnetEmpty").GetComponent<Image>();
        input = transform.Find("input").GetComponent<InputField>();
        Btn_xuanguan = transform.Find("Btn_xuanguan").GetComponent<Button>();

        m_Show = false;
        input.gameObject.SetActive(m_Show);
    }

    public override void OnViewShow(EventArgsPack args)
    {
        base.OnViewShow(args);
        RegistEvent();

        Init();
    }

    public async UniTaskVoid Init()
    {
        var itemUsage = YooAssets.LoadAssetAsync<TextAsset>("ItemUsageConfig");
        itemUsage.Completed += (handle) =>
        {
            var res = handle.AssetObject as TextAsset;
            GameConfig.Instance.LoadLocalItemUsageConfig(res.text);
        };
        await itemUsage.ToUniTask();

        RefreshAllUIStatus();
    }

    public override void OnViewUpdate()
    {
        base.OnViewUpdate();

    }
    public override void OnViewUpdateBySecond()
    {
        base.OnViewUpdateBySecond();

    }
    public override void OnViewHide()
    {
        base.OnViewHide();

        UnregistEvent();
    }
    public override void OnViewDestroy()
    {
        base.OnViewDestroy();

    }

    public void RegistEvent()
    {
        Btn_Setting.onClick.AddListener(OnBtn_SettingClicked);
        Btn_Bank.onClick.AddListener(OnBtn_BankClicked);
        Btn_AddBox.onClick.AddListener(OnBtn_AddBoxClicked);
        Btn_Clear.onClick.AddListener(OnBtn_ClearClicked);
        Btn_Magnet.onClick.AddListener(OnBtn_MagnetClicked);
        Btn_xuanguan.onClick.AddListener(OnBtn_xuanguan);

        input.onEndEdit.AddListener(LevelSelect);
        EventManager.Instance.AddEvent(GameEventGlobalDefine.OnGetGameTools, OnGetGetToolsEvent);
    }

    private bool m_Show = false;
    public void OnBtn_xuanguan()
    {
        m_Show = !m_Show;
        input.gameObject.SetActive(m_Show);
    }
    public void LevelSelect(string str)
    {
        Manager.GameManager.Instance.LevelSelect(int.Parse(str));
        OnBtn_xuanguan();
    }
    /// <summary>
    /// 0:道具id
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void OnGetGetToolsEvent(object sender, EventArgsPack e)
    {
        var itemId = (ItemID)e.GetData<int>(0);

        var usageData = GameConfig.LocalItemUsageManager.GetItemUsageConfigData((int)itemId);
        usageData.currentCount = usageData.defaultCount;
        usageData.currentAcquireCount++;
        RefreshItemUIStatus(itemId);

    }

    public void RefreshAllUIStatus()
    {
        RefreshItemUIStatus(ItemID.AddSlot);
        RefreshItemUIStatus(ItemID.Clear);
        RefreshItemUIStatus(ItemID.Magnet);
    }

    public void RefreshItemUIStatus(ItemID itemId)
    {
        var usageData = GameConfig.LocalItemUsageManager.GetItemUsageConfigData((int)itemId);
        if (usageData == null)
        {
            console.error("错误", itemId);
            return;
        }

        switch (itemId)
        {
            case ItemID.AddSlot:
                {
                    if (usageData.currentCount <= 0)
                    {
                        Txt_AddBoxCount.text = "+";
                        if (usageData.currentAcquireCount > usageData.maxAcquireCount)
                        {
                            Img_AddBoxCount.gameObject.SetActive(false);
                            Btn_AddBox.GetComponent<Image>().SetMaterialWithAsync("UIGray");
                        }
                        else
                        {
                            Img_AddBoxCount.gameObject.SetActive(true);
                            Btn_AddBox.GetComponent<Image>().material = null;
                        }
                    }
                    else
                    {
                        Txt_AddBoxCount.text = usageData.currentAcquireCount.ToString();
                        Img_AddBoxCount.gameObject.SetActive(false);
                        Btn_AddBox.GetComponent<Image>().material = null;
                    }
                    Img_AddBoxEmpty.gameObject.SetActive(usageData.currentCount <= 0);
                }
                break;
            case ItemID.Clear:
                {
                    if (usageData.currentCount <= 0)
                    {
                        Txt_ClearCount.text = "+";
                        if (usageData.currentAcquireCount > usageData.maxAcquireCount)
                        {
                            Img_ClearCount.gameObject.SetActive(false);
                            Btn_Clear.GetComponent<Image>().SetMaterialWithAsync("UIGray");
                        }
                        else
                        {
                            Img_ClearCount.gameObject.SetActive(true);
                            Btn_Clear.GetComponent<Image>().material = null;
                        }
                    }
                    else
                    {
                        Txt_ClearCount.text = usageData.currentAcquireCount.ToString();
                        Img_ClearCount.gameObject.SetActive(false);
                        Btn_Clear.GetComponent<Image>().material = null;
                    }
                    Img_ClearEmpty.gameObject.SetActive(usageData.currentCount <= 0);
                }
                break;
            case ItemID.Magnet:
                {
                    if (usageData.currentCount <= 0)
                    {
                        Txt_MagnetCount.text = "+";
                        if (usageData.currentAcquireCount > usageData.maxAcquireCount)
                        {
                            Img_MagnetCount.gameObject.SetActive(false);
                            Btn_Magnet.GetComponent<Image>().SetMaterialWithAsync("UIGray");
                        }
                        else
                        {
                            Img_MagnetCount.gameObject.SetActive(true);
                            Btn_Magnet.GetComponent<Image>().material = null;
                        }
                    }
                    else
                    {
                        Txt_MagnetCount.text = usageData.currentAcquireCount.ToString();
                        Img_MagnetCount.gameObject.SetActive(false);
                        Btn_Magnet.GetComponent<Image>().material = null;
                    }
                    Img_MagnetEmpty.gameObject.SetActive(usageData.currentCount <= 0);
                }
                break;
            default:
                break;
        }
    }

    public void OnBtn_SettingClicked()
    {
        UIManager.Instance.OpenUI(UIViewID.UISetting, new EventArgsPack(true));
    }

    public void OnBtn_BankClicked()
    {
        UIManager.Instance.OpenUI(UIViewID.UIBank);
    }

    public void OnBtn_AddBoxClicked()
    {

    }

    public void OnBtn_ClearClicked()
    {

    }

    public void OnBtn_MagnetClicked()
    {

    }

    public void UnregistEvent()
    {
        Btn_Setting.onClick.RemoveListener(OnBtn_SettingClicked);
        Btn_Bank.onClick.RemoveListener(OnBtn_BankClicked);
        Btn_AddBox.onClick.RemoveListener(OnBtn_AddBoxClicked);
        Btn_Clear.onClick.RemoveListener(OnBtn_ClearClicked);
        Btn_Magnet.onClick.RemoveListener(OnBtn_MagnetClicked);
        input.onEndEdit.RemoveListener(LevelSelect);
        Btn_xuanguan.onClick.RemoveListener(OnBtn_xuanguan);
        
        EventManager.Instance.RemoveEvent(GameEventGlobalDefine.OnGetGameTools, OnGetGetToolsEvent);
    }


}
