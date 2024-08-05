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

    public Image Img_Bg;
    public Image Img_House;
    public Image Img_Bord;
    public Text Txt_InterestRate;
    public Button Btn_StartGame;
    public Button Btn_Rank;
    public Button Btn_Collect;
    public Button Btn_Setting;
    public Button Btn_Bank;

    public Image Img_Cloud1;
    public Image Img_Cloud2;


    private List<Image> m_CloudList;
    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);
        Img_Bg = transform.Find("Img_Bg").GetComponent<Image>();
        Img_Cloud1 = transform.Find("Img_Bg/Img_Cloud1").GetComponent<Image>();
        Img_Cloud2 = transform.Find("Img_Bg/Img_Cloud2").GetComponent<Image>();
        Img_House = transform.Find("Img_Bg/Img_House").GetComponent<Image>();
        Img_Bord = transform.Find("Img_Bg/Img_Bord").GetComponent<Image>();
        Txt_InterestRate = transform.Find("Img_Bg/Img_Bord/Txt_InterestRate").GetComponent<Text>();
        Btn_StartGame = transform.Find("Btn_StartGame").GetComponent<Button>();
        Btn_Rank = transform.Find("Btn_Rank").GetComponent<Button>();
        Btn_Collect = transform.Find("Btn_Collect").GetComponent<Button>();
        Btn_Setting = transform.Find("Btn_Setting").GetComponent<Button>();
        Btn_Bank = transform.Find("Btn_Bank").GetComponent<Button>();

        Img_Bg.GetComponent<RectTransform>().offsetMin = -UIManager.Instance.FullOffset.offsetMin;
        Img_Bg.GetComponent<RectTransform>().offsetMax = -UIManager.Instance.FullOffset.offsetMax;

        m_CloudList = new List<Image>();
        m_CloudList.Add(Img_Cloud1);
        m_CloudList.Add(Img_Cloud2);
    }
    public override void OnViewShow(EventArgsPack args)
    {
        base.OnViewShow(args);
        RegistEvent();
    }
    public override void OnViewUpdate()
    {
        base.OnViewUpdate();

        var len = m_CloudList.Count;
        for (int i = 0; i < len; i++)
        {
            var cloud = m_CloudList[i];
            cloud.rectTransform.anchoredPosition = new Vector2(cloud.rectTransform.anchoredPosition.x - 15 * Time.deltaTime, cloud.rectTransform.anchoredPosition.y);
            if(cloud.rectTransform.anchoredPosition.x < -(UIManager.Instance.FullSizeDetail.x * 0.5f + cloud.rectTransform.sizeDelta.x * 1.5))
            {
                cloud.rectTransform.anchoredPosition = new Vector2((UIManager.Instance.FullSizeDetail.x * 0.5f + 50), cloud.rectTransform.anchoredPosition.y);
            }
        }

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

    public RectTransform ss;
    public RectTransform ee;

    public void OnBtn_StartGameClicked()
    {
        EventManager.Instance.DispatchEvent(GameEventGlobalDefine.EnterGamePage, null, null);
        //UIFlyElementSys.Instance.PlayFlyCoinEffect(ss.position, ee.position);
        //args[0]：是否带图片类型， args[1]：desc， args[2]：动画播放完成回调 args[3]：点击遮罩关闭回调
        //UIManager.Instance.OpenUI(UIViewID.UITips, new EventArgsPack(true, "你好，你的利息很高很高",
        //    new Action(() => { console.info("动画播放回调", this.name); }),
        //    new Action(() => { console.info("点击后的回调"); })));
    }


    public void OnBtn_RankClicked()
    {

    }

    public void OnBtn_CollectClicked()
    {
        UIManager.Instance.OpenUI(UIViewID.UICollect);
    }

    public void OnBtn_SettingClicked()
    {
        UIManager.Instance.OpenUI(UIViewID.UISetting, new EventArgsPack(false));
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
