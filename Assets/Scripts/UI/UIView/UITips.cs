using Joyland.GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.UI;

public class UITips : UIViewBase
{
    public Image Img_Bg;
    public GameObject Obj_Container;
    public GameObject Obj_HasImageType;
    public Text Txt_HasImageTypeDesc;
    public GameObject Obj_NoImageType;
    public Text Txt_NoImageTypeDesc;


    private bool m_IsHasImageType = false;
    private string m_Desc = string.Empty;
    private Action m_AnimationFinishCb = null;
    private Action m_ClickCloseFinishCb = null;
    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);
        Img_Bg = transform.Find("Img_Bg").GetComponent<Image>();
        Obj_Container = transform.Find("Obj_Container").gameObject;
        Obj_HasImageType = transform.Find("Obj_Container/Obj_HasImageType").gameObject;
        Txt_HasImageTypeDesc = transform.Find("Obj_Container/Obj_HasImageType/GameObject/Txt_HasImageTypeDesc").GetComponent<Text>();
        Obj_NoImageType = transform.Find("Obj_Container/Obj_NoImageType").gameObject;
        Txt_NoImageTypeDesc = transform.Find("Obj_Container/Obj_NoImageType/GameObject/Txt_NoImageTypeDesc").GetComponent<Text>();

        Img_Bg.GetComponent<RectTransform>().offsetMin = UIManager.Instance.FullOffset.offsetMin;
        Img_Bg.GetComponent<RectTransform>().offsetMax = UIManager.Instance.FullOffset.offsetMax;

        //参数：  args[0]：是否带图片类型， args[1]：desc， args[2]：动画播放完成回调 args[3]：点击遮罩关闭回调

        if (args.ArgsLength > 0)
        {
            m_IsHasImageType = args.GetData<bool>(0);
            m_Desc = args.GetData<string>(1);
            m_AnimationFinishCb = args.GetData<Action>(2);
            m_ClickCloseFinishCb = args.GetData<Action>(3);
        }

    }

    public override void OnViewShow(EventArgsPack args)
    {
        base.OnViewShow(args);
        RegistEvent();

        Obj_HasImageType.SetActive(m_IsHasImageType);
        Obj_NoImageType.SetActive(m_IsHasImageType);

        Txt_HasImageTypeDesc.text = m_Desc;
        Txt_NoImageTypeDesc.text = m_Desc;
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

    }


    public void UnregistEvent()
    {

    }


}
