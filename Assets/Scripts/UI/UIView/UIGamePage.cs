using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.UI;

public class UIGamePage : UIViewBase
{
    public Button Btn_Setting;
    public Button Btn_Bank;
    public Button Btn_AddBox;
    public Text Txt_AddBoxCount;
    public Image Img_AddBoxEmpty;
    public Button Btn_Clear;
    public Text Txt_ClearCount;
    public Image Img_ClearEmpty;
    public Button Btn_Magnet;
    public Text Txt_MagnetCount;
    public Image Img_MagnetEmpty;

    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);
        Btn_Setting = transform.Find("Btn_Setting").GetComponent<Button>();
        Btn_Bank = transform.Find("Btn_Bank").GetComponent<Button>();
        Btn_AddBox = transform.Find("Btn_AddBox").GetComponent<Button>();
        Txt_AddBoxCount = transform.Find("Btn_AddBox/Image/Txt_AddBoxCount").GetComponent<Text>();
        Img_AddBoxEmpty = transform.Find("Btn_AddBox/Image/Img_AddBoxEmpty").GetComponent<Image>();
        Btn_Clear = transform.Find("Btn_Clear").GetComponent<Button>();
        Txt_ClearCount = transform.Find("Btn_Clear/Image/Txt_ClearCount").GetComponent<Text>();
        Img_ClearEmpty = transform.Find("Btn_Clear/Image/Img_ClearEmpty").GetComponent<Image>();
        Btn_Magnet = transform.Find("Btn_Magnet").GetComponent<Button>();
        Txt_MagnetCount = transform.Find("Btn_Magnet/Image/Txt_MagnetCount").GetComponent<Text>();
        Img_MagnetEmpty = transform.Find("Btn_Magnet/Image/Img_MagnetEmpty").GetComponent<Image>();


    }

    public override void OnViewShow(EventArgsPack args)
    {
        base.OnViewShow(args);
        RegistEvent();
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

        EventManager.Instance.AddEvent(GameEventGlobalDefine.OnGetGameTools, OnGetGetToolsEvent);
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

    public void RefreshItemUIStatus(ItemID itemID)
    {

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

        EventManager.Instance.RemoveEvent(GameEventGlobalDefine.OnGetGameTools, OnGetGetToolsEvent);
    }


}
