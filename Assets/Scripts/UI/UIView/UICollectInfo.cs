using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.UI;

public class UICollectInfo : UIViewBase
{
    private int _UIOrder = 103;
    public override int UIOrder
    {
        get { return _UIOrder; }
        set { _UIOrder = value; }
    }

    public Image Img_Bg;
    public GameObject Obj_Container;
    public Image Img_ContainerBg;
    public Button Btn_Close;
    public Text Txt_UserNameDesc;
    public Image RawImg_Avatar;
    public Text Txt_TotalCoins;
    public Text Txt_Desc1;
    public Text Txt_Desc2;


    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);
        Img_Bg = transform.Find("Img_Bg").GetComponent<Image>();
        Obj_Container = transform.Find("Obj_Container").gameObject;
        Img_ContainerBg = transform.Find("Obj_Container/Img_ContainerBg").GetComponent<Image>();
        Btn_Close = transform.Find("Obj_Container/Btn_Close").GetComponent<Button>();
        Txt_UserNameDesc = transform.Find("Obj_Container/Txt_UserNameDesc").GetComponent<Text>();
        RawImg_Avatar = transform.Find("Obj_Container/Image/RawImg_Avatar").GetComponent<Image>();
        Txt_TotalCoins = transform.Find("Obj_Container/Txt_TotalCoins").GetComponent<Text>();
        Txt_Desc1 = transform.Find("Obj_Container/GameObject/Txt_Desc1").GetComponent<Text>();
        Txt_Desc2 = transform.Find("Obj_Container/GameObject/Txt_Desc2").GetComponent<Text>();

        Img_Bg.GetComponent<RectTransform>().offsetMin = UIManager.Instance.FullOffset.offsetMin;
        Img_Bg.GetComponent<RectTransform>().offsetMax = UIManager.Instance.FullOffset.offsetMax;


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
        UIManager.Instance.CloseUI(UIViewID.UICollectInfo);
    }

    public void UnregistEvent()
    {
        Btn_Close.onClick.RemoveListener(OnBtn_CloseClicked);

    }


}
