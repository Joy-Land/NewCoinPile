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

    private int m_ItemIdx;
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

        Img_Bg.GetComponent<RectTransform>().offsetMin = -UIManager.Instance.FullOffset.offsetMin;
        Img_Bg.GetComponent<RectTransform>().offsetMax = -UIManager.Instance.FullOffset.offsetMax;

        if (args.ArgsLength > 0)
        {
            m_ItemIdx = args.GetData<int>(0);
        }
    }
    public override void OnViewShow(EventArgsPack args)
    {
        base.OnViewShow(args);
        RegistEvent();

        var idx = m_ItemIdx;

        var conf = GameConfig.LocalCollectManager.AllCollectItemConfigDatas[idx];

        var testCurrentCoins = 90;

        Txt_TotalCoins.text = testCurrentCoins.ToString() + "万";
        Txt_Desc1.text = string.Format(GameConfig.LocalCopyWriteManager.AllCopyWriteConfigDatas["collect_desc1"], conf.desc);
        Txt_Desc2.text = string.Format(GameConfig.LocalCopyWriteManager.AllCopyWriteConfigDatas["collect_desc2"], conf.note);

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
