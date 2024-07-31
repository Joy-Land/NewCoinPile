using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.UI;

public class UIBank : UIViewBase
{
    private int _UIOrder = 101;
    public override int UIOrder
    {
        get { return _UIOrder; }
        set { _UIOrder = value; }
    }


    public Image Img_Bg;
    public GameObject Obj_Container;
    public Image Img_ContainerBg;
    public Image Img_Title;
    public Text Txt_Title;
    public Button Btn_Close;
    public GameObject Obj_CurrentInfo;
    public Image Img_BackIcon;
    public Text Txt_Date;
    public Text Txt_DateTime;
    public Text Txt_TodayIntersetRate;
    public GameObject Obj_LastEarnings;
    public Text Txt_LastEarningCoins;
    public GameObject Obj_TotalAssets;
    public Text Txt_TotalCoins;


    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);
        Img_Bg = transform.Find("Img_Bg").GetComponent<Image>();
        Obj_Container = transform.Find("Obj_Container").gameObject;
        Img_ContainerBg = transform.Find("Obj_Container/Img_ContainerBg").GetComponent<Image>();
        Img_Title = transform.Find("Obj_Container/Img_Title").GetComponent<Image>();
        Txt_Title = transform.Find("Obj_Container/Img_Title/Txt_Title").GetComponent<Text>();
        Btn_Close = transform.Find("Obj_Container/Btn_Close").GetComponent<Button>();
        Obj_CurrentInfo = transform.Find("Obj_Container/Obj_CurrentInfo").gameObject;
        Img_BackIcon = transform.Find("Obj_Container/Obj_CurrentInfo/Img_BackIcon").GetComponent<Image>();
        Txt_Date = transform.Find("Obj_Container/Obj_CurrentInfo/Image/Image/Txt_Date").GetComponent<Text>();
        Txt_DateTime = transform.Find("Obj_Container/Obj_CurrentInfo/Image/Txt_DateTime").GetComponent<Text>();
        Txt_TodayIntersetRate = transform.Find("Obj_Container/Obj_CurrentInfo/Image/Image (1)/Txt_TodayIntersetRate").GetComponent<Text>();
        Obj_LastEarnings = transform.Find("Obj_Container/Obj_LastEarnings").gameObject;
        Txt_LastEarningCoins = transform.Find("Obj_Container/Obj_LastEarnings/Image/Txt_LastEarningCoins").GetComponent<Text>();
        Obj_TotalAssets = transform.Find("Obj_Container/Obj_TotalAssets").gameObject;
        Txt_TotalCoins = transform.Find("Obj_Container/Obj_TotalAssets/Image/Txt_TotalCoins").GetComponent<Text>();

        Img_Bg.GetComponent<RectTransform>().offsetMin = -UIManager.Instance.FullOffset.offsetMin;
        Img_Bg.GetComponent<RectTransform>().offsetMax = -UIManager.Instance.FullOffset.offsetMax;


    }

    public override void SetAnimatorNode()
    {
        base.SetAnimatorNode();

        if (this.animator == null)
        {
            if (Obj_Container.GetComponent<Animator>() == null)
            {
                this.animator = Obj_Container.AddComponent<Animator>();
            }
        }
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
        Btn_Close.onClick.AddListener(OnBtn_CloseClicked);

    }


    public void OnBtn_CloseClicked()
    {
        UIManager.Instance.CloseUI(UIViewID.UIBank);
    }

    public void UnregistEvent()
    {
        Btn_Close.onClick.RemoveListener(OnBtn_CloseClicked);

    }


}
