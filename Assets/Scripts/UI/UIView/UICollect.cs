
using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

public class UICollect : UIViewBase
{
    private int _UIOrder = 102;
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
    public ScrollRect ScrollView_CollectItems;
    public GameObject Obj_CollectItemsContent;


    private List<UICollectItemComponent> m_CollectItemCompList= new List<UICollectItemComponent>();
    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);
        Img_Bg = transform.Find("Img_Bg").GetComponent<Image>();
        Obj_Container = transform.Find("Obj_Container").gameObject;
        Img_ContainerBg = transform.Find("Obj_Container/Img_ContainerBg").GetComponent<Image>();
        Img_Title = transform.Find("Obj_Container/Img_Title").GetComponent<Image>();
        Txt_Title = transform.Find("Obj_Container/Img_Title/Txt_Title").GetComponent<Text>();
        Btn_Close = transform.Find("Obj_Container/Btn_Close").GetComponent<Button>();
        ScrollView_CollectItems = transform.Find("Obj_Container/ScrollView_CollectItems").GetComponent<ScrollRect>();
        Obj_CollectItemsContent = transform.Find("Obj_Container/ScrollView_CollectItems/Viewport/Obj_CollectItemsContent").gameObject;

        Img_Bg.GetComponent<RectTransform>().offsetMin = UIManager.Instance.FullOffset.offsetMin;
        Img_Bg.GetComponent<RectTransform>().offsetMax = UIManager.Instance.FullOffset.offsetMax;


        //读取配置们，然后展示
        var list = GameConfig.LocalCollectManager.AllCollectItemConfigDatas;
        var len = list.Count;

        for (int i = 0; i < list.Count; i++)
        {
            var confData = list[i];
            var collectItemComp = UIManager.Instance.CreateUIComponent<UICollectItemComponent>(UICompID.UICollectItemComponent, Obj_CollectItemsContent.transform);
            m_CollectItemCompList.Add(collectItemComp);

            //TODO:根据元子数量，设置每个comp状态
            collectItemComp.SetData(new EventArgsPack(i));
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

        foreach (var item in m_CollectItemCompList)
        {
            UIManager.Instance.DestroyUIComponent(UICompID.UICollectItemComponent, item);
        }
    }

    public void RegistEvent()
    {
        Btn_Close.onClick.AddListener(OnBtn_CloseClicked);

    }


    public void OnBtn_CloseClicked()
    {
        UIManager.Instance.CloseUI(UIViewID.UICollect);
    }

    public void UnregistEvent()
    {
        Btn_Close.onClick.RemoveListener(OnBtn_CloseClicked);

    }




}
