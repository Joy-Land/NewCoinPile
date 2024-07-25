using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.UI;

public class UIPopupItem : UIViewBase
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
    public Text Txt_Desc;
    public Image Img_ItemIcon;
    public Button Btn_TryGetItem;
    public Text Txt_TryGetItem;
    public Image Img_TryGetTypeIcon;

    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);
        Img_Bg = transform.Find("Img_Bg").GetComponent<Image>();
        Obj_Container = transform.Find("Obj_Container").gameObject;
        Img_ContainerBg = transform.Find("Obj_Container/Img_ContainerBg").GetComponent<Image>();
        Img_Title = transform.Find("Obj_Container/Img_Title").GetComponent<Image>();
        Txt_Title = transform.Find("Obj_Container/Img_Title/Txt_Title").GetComponent<Text>();
        Btn_Close = transform.Find("Obj_Container/Btn_Close").GetComponent<Button>();
        Txt_Desc = transform.Find("Obj_Container/Txt_Desc").GetComponent<Text>();
        Img_ItemIcon = transform.Find("Obj_Container/Img_ItemIcon").GetComponent<Image>();
        Btn_TryGetItem = transform.Find("Obj_Container/Btn_TryGetItem").GetComponent<Button>();
        Txt_TryGetItem = transform.Find("Obj_Container/Btn_TryGetItem/Txt_TryGetItem").GetComponent<Text>();
        Img_TryGetTypeIcon = transform.Find("Obj_Container/Btn_TryGetItem/Img_TryGetTypeIcon").GetComponent<Image>();

        Img_Bg.GetComponent<RectTransform>().offsetMin = UIManager.Instance.FullOffset.offsetMin;
        Img_Bg.GetComponent<RectTransform>().offsetMax = UIManager.Instance.FullOffset.offsetMax;
    }

    public override void SetAnimatorNode()
    {
        base.SetAnimatorNode();

        if (this.animator == null)
        {
            this.animator = Obj_Container.GetComponent<Animator>() ?? Obj_Container.AddComponent<Animator>();
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
        Btn_TryGetItem.onClick.AddListener(OnBtn_TryGetItemClicked);

    }


    public void OnBtn_CloseClicked()
    {

    }

    public void OnBtn_TryGetItemClicked()
    {

    }

    public void UnregistEvent()
    {
        Btn_Close.onClick.RemoveListener(OnBtn_CloseClicked);
        Btn_TryGetItem.onClick.RemoveListener(OnBtn_TryGetItemClicked);

    }



}
