using Joyland.GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.UI;

public class UIHomePage : UIViewBase
{
    private int _UIOrder = 101;
    public override int UIOrder
    {
        get { return _UIOrder; }
        set { _UIOrder = value; }
    }


    public Button Btn_StartGame;
    public Button Btn_Rank;
    public Button Btn_Collect;
    public Button Btn_Setting;
    public Button Btn_Bank;


    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);
        Btn_StartGame = transform.Find("Btn_StartGame").GetComponent<Button>();
        Btn_Rank = transform.Find("Btn_Rank").GetComponent<Button>();
        Btn_Collect = transform.Find("Btn_Collect").GetComponent<Button>();
        Btn_Setting = transform.Find("Btn_Setting").GetComponent<Button>();
        Btn_Bank = transform.Find("Btn_Bank").GetComponent<Button>();


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
        Btn_StartGame.onClick.AddListener(OnBtn_StartGameClicked);
        Btn_Rank.onClick.AddListener(OnBtn_RankClicked);
        Btn_Collect.onClick.AddListener(OnBtn_CollectClicked);
        Btn_Setting.onClick.AddListener(OnBtn_SettingClicked);
        Btn_Bank.onClick.AddListener(OnBtn_BankClicked);

    }

    

    public void OnBtn_StartGameClicked()
    {

        //args[0]：是否带图片类型， args[1]：desc， args[2]：动画播放完成回调 args[3]：点击遮罩关闭回调
        //UIManager.Instance.OpenUI(UIViewID.UITips, new EventArgsPack(true, "你好，你的利息很高很高",
        //    new Action(() => { console.info("动画播放回调", this.name); }),
        //    new Action(() => { console.info("点击后的回调"); })));
    }

    public void EventArgsPack(Delegate @delegate)
    {
        //m_Args = new object[] { @delegate };
    }

    public void OnBtn_RankClicked()
    {

    }

    public void OnBtn_CollectClicked()
    {

    }

    public void OnBtn_SettingClicked()
    {
        UIManager.Instance.OpenUI(UIViewID.UISetting);
    }

    public void OnBtn_BankClicked()
    {
        UIManager.Instance.OpenUI(UIViewID.UIBank);
    }

    public void UnregistEvent()
    {
        Btn_StartGame.onClick.RemoveListener(OnBtn_StartGameClicked);
        Btn_Rank.onClick.RemoveListener(OnBtn_RankClicked);
        Btn_Collect.onClick.RemoveListener(OnBtn_CollectClicked);
        Btn_Setting.onClick.RemoveListener(OnBtn_SettingClicked);
        Btn_Bank.onClick.RemoveListener(OnBtn_BankClicked);

    }



}
