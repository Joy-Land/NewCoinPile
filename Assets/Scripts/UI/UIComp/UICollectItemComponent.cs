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
    public Text Txt_LockDesc;
    public Button Btn_Node;

    private int m_ItemIdx = 0;
    public override void OnComponentInit()
    {
        base.OnComponentInit();
        Obj_Unlock = transform.Find("Obj_Unlock").gameObject;
        Txt_Desc1 = transform.Find("Obj_Unlock/Txt_Desc1").GetComponent<Text>();
        Txt_Desc2 = transform.Find("Obj_Unlock/Txt_Desc2").GetComponent<Text>();
        Obj_Lock = transform.Find("Obj_Lock").gameObject;
        Txt_LockDesc = transform.Find("Obj_Lock/Txt_LockDesc").GetComponent<Text>();
        Btn_Node = gameObject.GetComponent<Button>();

        RegistEvent();
    }
    public override void OnComponentShow()
    {
        base.OnComponentShow();

    }

    /// <summary>
    /// 0:当前配置索引
    /// </summary>
    /// <param name="args"></param>
    public override void SetData(EventArgsPack args)
    {
        base.SetData(args);


        var testCurrentCoins = 90;
        m_ItemIdx = args.GetData<int>(0);
        var idx = m_ItemIdx;

        var conf = GameConfig.LocalCollectManager.AllCollectItemConfigDatas[idx];

        Obj_Lock.SetActive(testCurrentCoins <= conf.limit);
        Obj_Unlock.SetActive(testCurrentCoins > conf.limit);

        Txt_Desc1.text = string.Format(GameConfig.LocalCopyWriteManager.AllCopyWriteConfigDatas["collect_desc1"], conf.desc);
        Txt_Desc2.text = string.Format(GameConfig.LocalCopyWriteManager.AllCopyWriteConfigDatas["collect_desc2"], conf.note);
        Txt_LockDesc.text = string.Format("总资产达到{0}解锁", conf.limit);
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
        UIManager.Instance.OpenUI(UIViewID.UICollectInfo, new EventArgsPack(m_ItemIdx));
    }

    public void UnregistEvent()
    {
        Btn_Node.onClick.RemoveListener(OnItemClicked);
    }


}
