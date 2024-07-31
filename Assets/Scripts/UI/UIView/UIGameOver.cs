using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.UI;

public class UIGameOver : UIViewBase
{
    private int _UIOrder = 100;
    public override int UIOrder
    {
        get { return _UIOrder; }
        set { _UIOrder = value; }
    }
    public Image Img_Bg;
    public GameObject Obj_Container;
    public Image Img_ContainerBg;
    public Button Btn_Close;
    public GameObject Obj_Success;
    public Text Txt_GetCoinsCount;
    public Button Btn_TryGetMoreCoins;
    public Text Txt_TryGetMoreCoins;
    public Image Img_TryGetTypeMoreCoins;
    public Button Btn_TryGetCoins;
    public Text Txt_TryGetCoins;
    public GameObject Obj_Fail;
    public Button Btn_TryGetRevive;
    public Text Txt_TryGetRevive;
    public Image Img_TryGetTypeRevive;

    private bool m_IsSucces = false;
    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);
        Img_Bg = transform.Find("Img_Bg").GetComponent<Image>();
        Obj_Container = transform.Find("Obj_Container").gameObject;
        Img_ContainerBg = transform.Find("Obj_Container/Img_ContainerBg").GetComponent<Image>();
        Btn_Close = transform.Find("Obj_Container/Btn_Close").GetComponent<Button>();
        Obj_Success = transform.Find("Obj_Container/Obj_Success").gameObject;
        Txt_GetCoinsCount = transform.Find("Obj_Container/Obj_Success/Txt_GetCoinsCount").GetComponent<Text>();
        Btn_TryGetMoreCoins = transform.Find("Obj_Container/Obj_Success/Btn_TryGetMoreCoins").GetComponent<Button>();
        Txt_TryGetMoreCoins = transform.Find("Obj_Container/Obj_Success/Btn_TryGetMoreCoins/Txt_TryGetMoreCoins").GetComponent<Text>();
        Img_TryGetTypeMoreCoins = transform.Find("Obj_Container/Obj_Success/Btn_TryGetMoreCoins/Img_TryGetTypeMoreCoins").GetComponent<Image>();
        Btn_TryGetCoins = transform.Find("Obj_Container/Obj_Success/Btn_TryGetCoins").GetComponent<Button>();
        Txt_TryGetCoins = transform.Find("Obj_Container/Obj_Success/Btn_TryGetCoins/Txt_TryGetCoins").GetComponent<Text>();
        Obj_Fail = transform.Find("Obj_Container/Obj_Fail").gameObject;
        Btn_TryGetRevive = transform.Find("Obj_Container/Obj_Fail/Btn_TryGetRevive").GetComponent<Button>();
        Txt_TryGetRevive = transform.Find("Obj_Container/Obj_Fail/Btn_TryGetRevive/Txt_TryGetRevive").GetComponent<Text>();
        Img_TryGetTypeRevive = transform.Find("Obj_Container/Obj_Fail/Btn_TryGetRevive/Img_TryGetTypeRevive").GetComponent<Image>();

        Img_Bg.GetComponent<RectTransform>().offsetMin = UIManager.Instance.FullOffset.offsetMin;
        Img_Bg.GetComponent<RectTransform>().offsetMax = UIManager.Instance.FullOffset.offsetMax;

        if(args.ArgsLength>0)
        {
            m_IsSucces = args.GetData<bool>(0);
        }
        
        //Test------------------------------------
        Btn_TryGetMoreCoins.gameObject.SetActive(false);
        Btn_TryGetCoins.GetComponentInChildren<Text>().text = "进入下一关";
    }
    public override void OnViewShow(EventArgsPack args)
    {
        base.OnViewShow(args);
        RegistEvent();

        if (m_IsSucces)
        {
            Obj_Success.SetActive(true);
            Obj_Fail.SetActive(false);
        }
        else
        {
            Obj_Success.SetActive(false);
            Obj_Fail.SetActive(true);
        }

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
        Btn_Close.onClick.AddListener(OnBtn_CloseClicked);
        Btn_TryGetMoreCoins.onClick.AddListener(OnBtn_TryGetMoreCoinsClicked);
        Btn_TryGetCoins.onClick.AddListener(OnBtn_TryGetCoinsClicked);
        Btn_TryGetRevive.onClick.AddListener(OnBtn_TryGetReviveClicked);

    }


    public void OnBtn_CloseClicked()
    {
        //关闭页面
        UIManager.Instance.CloseUI(UIViewID.UIGameOver);
        //跳转到结算
        UIManager.Instance.OpenUI(UIViewID.UIGameSettlement);
    }

    public void OnBtn_TryGetMoreCoinsClicked()
    {
        //看广告

        //判断自己资产是否到了下一级别
        //如果没到，直接到下一关
        //如果到了，进入结算界面
    }

    public void OnBtn_TryGetCoinsClicked()
    {
        //判断自己资产是否到了下一级别
        //如果没到，直接到下一关
        //如果到了，进入结算界面
        
        
        //Test----------------
        UIManager.Instance.CloseUI(UIViewID.UIGameOver);
        Manager.GameManager.Instance.ReStart(true);
    }

    public void OnBtn_TryGetReviveClicked()
    {
        //做通知局内事件复活，局内处理完事件看完广告再关掉这个gameover界面
    }

    public void UnregistEvent()
    {
        Btn_Close.onClick.RemoveListener(OnBtn_CloseClicked);
        Btn_TryGetMoreCoins.onClick.RemoveListener(OnBtn_TryGetMoreCoinsClicked);
        Btn_TryGetCoins.onClick.RemoveListener(OnBtn_TryGetCoinsClicked);
        Btn_TryGetRevive.onClick.RemoveListener(OnBtn_TryGetReviveClicked);

    }


}
