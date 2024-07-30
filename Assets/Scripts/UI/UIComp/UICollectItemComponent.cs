using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.UI;

public class UICollectItemComponent : UICompBase
{
    public GameObject Obj_Unlock;
    public Text Txt_Desc1;
    public Text Txt_Desc2;
    public GameObject Obj_Lock;
    public Image Img_LockDesc;
    public Button Btn_Node;

    public override void OnComponentInit()
    {
        base.OnComponentInit();
        Obj_Unlock = transform.Find("Obj_Unlock").gameObject;
        Txt_Desc1 = transform.Find("Obj_Unlock/Txt_Desc1").GetComponent<Text>();
        Txt_Desc2 = transform.Find("Obj_Unlock/Txt_Desc2").GetComponent<Text>();
        Obj_Lock = transform.Find("Obj_Lock").gameObject;
        Img_LockDesc = transform.Find("Obj_Lock/Img_LockDesc").GetComponent<Image>();
        Btn_Node = gameObject.GetComponent<Button>();

        RegistEvent();
    }
    public override void OnComponentShow()
    {
        base.OnComponentShow();

    }

    /// <summary>
    /// 0:需要解锁金币数
    /// </summary>
    /// <param name="args"></param>
    public override void SetData(EventArgsPack args)
    {
        base.SetData(args);

        args.GetData<float>(0);
    }
    public override void OnComponentHide()
    {
        base.OnComponentHide();

    }
    public override void OnComponentDestroy()
    {
        base.OnComponentDestroy();

        UnregistEvent();
    }

    public void RegistEvent()
    {
        Btn_Node.onClick.AddListener(OnItemClicked);
    }

    public void OnItemClicked()
    {
        //打开CollectInof界面
        UIManager.Instance.OpenUI(UIViewID.UICollectInfo);
    }

    public void UnregistEvent()
    {
        Btn_Node.onClick.RemoveListener(OnItemClicked);
    }


}
