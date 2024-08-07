using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

public class UIPopupItem : UIViewBase
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
    public Image Img_Title;
    public Text Txt_Title;
    public Button Btn_Close;
    public Text Txt_Desc;
    public Image Img_ItemIcon;
    public Button Btn_TryGetItem;
    public Text Txt_TryGetItem;
    public Image Img_TryGetTypeIcon;

    private Dictionary<ItemID, (string icon, string desc)> panelConf = new Dictionary<ItemID, (string icon, string desc)>() {
        {ItemID.AddSlot, ("","") },
        {ItemID.ReviveGame, ("","") },
        {ItemID.Magnet, ("","") },
        {ItemID.PlayGame, ("","") },
        {ItemID.Clear, ("","") },
    };

    private ItemID m_ItemID = 0;
    private (ItemCategoryID type, int remainNumber) m_ItemCategroyDate;
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

        Img_Bg.GetComponent<RectTransform>().offsetMin = -UIManager.Instance.FullOffset.offsetMin;
        Img_Bg.GetComponent<RectTransform>().offsetMax = -UIManager.Instance.FullOffset.offsetMax;

        if (args.ArgsLength>0)
        {
            m_ItemID = args.GetData<ItemID>(0);
            m_ItemCategroyDate = GameConfig.GameItemManager.GetItemCategoryData(m_ItemID);
        }
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

        var conf = GameConfig.LocalItemUsageManager.GetItemUsageConfigData((int)m_ItemID);
        Txt_Title.text = conf.itemName;
        Txt_Desc.text = conf.desc;

        Img_ItemIcon.SetImageWithAsync("ui_tools", panelConf[m_ItemID].icon);
        if(m_ItemCategroyDate.type == ItemCategoryID.Video)
        {
            Img_TryGetTypeIcon.SetImageWithAsync("ui_tools", "icon_ads");
        }
        else
        {
            Img_TryGetTypeIcon.SetImageWithAsync("ui_tools", "icon_share");
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
        Btn_TryGetItem.onClick.AddListener(OnBtn_TryGetItemClicked);

    }



    public void OnBtn_CloseClicked()
    {
        UIManager.Instance.CloseUI(UIViewID.UIPopupItem);
    }

    public void OnBtn_TryGetItemClicked()
    {
        if(m_ItemCategroyDate.type == ItemCategoryID.Share)
        {
            J.Minigame.Share(() =>
            {
                UIManager.Instance.CloseUI(UIViewID.UIPopupItem);
                GameConfig.GameItemManager.UpdateItemCategoryData(m_ItemID, m_ItemCategroyDate.type, -1);
                J.ReqHelper.UpdateItemData((int)m_ItemID, (int)m_ItemCategroyDate.type, m_ItemCategroyDate.remainNumber - 1, 0, (resData) =>
                {
                    GameConfig.Instance.SetItemData(resData);
                });
                EventManager.Instance.DispatchEvent(GameEventGlobalDefine.OnGetGameTools, null, new EventArgsPack(m_ItemID));
            });
        }
        else if(m_ItemCategroyDate.type == ItemCategoryID.Video)
        {
            UIManager.Instance.CloseUI(UIViewID.UIPopupItem);
            EventManager.Instance.DispatchEvent(GameEventGlobalDefine.OnGetGameTools, null, new EventArgsPack(m_ItemID));
        }
        else if(m_ItemCategroyDate.type == ItemCategoryID.Default)
        {
            UIManager.Instance.CloseUI(UIViewID.UIPopupItem);
            EventManager.Instance.DispatchEvent(GameEventGlobalDefine.OnGetGameTools, null, new EventArgsPack(m_ItemID));
        }

    }



    public void UnregistEvent()
    {
        Btn_Close.onClick.RemoveListener(OnBtn_CloseClicked);
        Btn_TryGetItem.onClick.RemoveListener(OnBtn_TryGetItemClicked);

        
    }



}
