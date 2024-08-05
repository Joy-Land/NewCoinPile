using DG.Tweening;
using Joyland.GamePlay;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.UI;

public class UIGameSettlement : UIViewBase
{

    private int _UIOrder = 101;
    public override int UIOrder
    {
        get { return _UIOrder; }
        set { _UIOrder = value; }
    }

    public Image Img_Bg;
    public GameObject Obj_Container;
    public GameObject Obj_Success;
    public Button Btn_Collect;
    public GameObject Obj_Txt_DescResult;
    public Text Txt_Desc1;
    public Text Txt_Desc2;
    public GameObject Obj_Fail;
    public Button Btn_TryGetRevive;
    public Text Txt_TryGetRevive;
    public Image Img_TryGetTypeRevive;
    public Button Btn_Close;

    private bool m_IsSuccess = false;
    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);
        Img_Bg = transform.Find("Img_Bg").GetComponent<Image>();
        Obj_Container = transform.Find("Obj_Container").gameObject;
        Obj_Success = transform.Find("Obj_Container/Obj_Success").gameObject;
        Btn_Collect = transform.Find("Obj_Container/Obj_Success/Btn_Collect").GetComponent<Button>();
        Obj_Txt_DescResult = transform.Find("Obj_Container/Obj_Success/Obj_TxtDescResult").gameObject;
        Txt_Desc1 = transform.Find("Obj_Container/Obj_Success/Obj_TxtDescResult/Txt_Desc1").GetComponent<Text>();
        Txt_Desc2 = transform.Find("Obj_Container/Obj_Success/Obj_TxtDescResult/Txt_Desc2").GetComponent<Text>();
        Obj_Fail = transform.Find("Obj_Container/Obj_Fail").gameObject;
        Btn_TryGetRevive = transform.Find("Obj_Container/Obj_Fail/Btn_TryGetRevive").GetComponent<Button>();
        Txt_TryGetRevive = transform.Find("Obj_Container/Obj_Fail/Btn_TryGetRevive/Txt_TryGetRevive").GetComponent<Text>();
        Img_TryGetTypeRevive = transform.Find("Obj_Container/Obj_Fail/Btn_TryGetRevive/Img_TryGetTypeRevive").GetComponent<Image>();
        Btn_Close = transform.Find("Obj_Container/Obj_Fail/Btn_Close").GetComponent<Button>();

        Img_Bg.GetComponent<RectTransform>().offsetMin = -UIManager.Instance.FullOffset.offsetMin;
        Img_Bg.GetComponent<RectTransform>().offsetMax = -UIManager.Instance.FullOffset.offsetMax;

        if (args.ArgsLength > 0)
        {
            m_IsSuccess = args.GetData<bool>(0);
        }
    }
    public override void OnViewShow(EventArgsPack args)
    {
        base.OnViewShow(args);
        RegistEvent();

        if (m_IsSuccess)
        {
            Obj_Success.SetActive(true);
            Obj_Fail.SetActive(false);

            Obj_Txt_DescResult.SetActive(false);
            Obj_Txt_DescResult.transform.localScale = Vector3.one * 5f;

            //盖章动画
            DOVirtual.DelayedCall(0.2f, () =>
            {
                Obj_Txt_DescResult.SetActive(true);
                Obj_Txt_DescResult.GetComponent<RectTransform>().DOScale(Vector3.one, 0.3f).SetDelay(0.07f).SetEase(Ease.Linear);
            });
            
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
        Btn_Collect.onClick.AddListener(OnBtn_CollectClicked);
        Btn_TryGetRevive.onClick.AddListener(OnBtn_TryGetReviveClicked);
        Btn_Close.onClick.AddListener(OnBtn_CloseClicked);

    }


    public void OnBtn_CollectClicked()
    {

    }

    public void OnBtn_TryGetReviveClicked()
    {
        Manager.GameManager.Instance.ReStart(false);
        UIManager.Instance.CloseUI(UIViewID.UIGameSettlement);
    }

    public void OnBtn_CloseClicked()
    {
        UIManager.Instance.CloseUI(UIViewID.UIGameSettlement);
        
        //切状态机到主页
        EventManager.Instance.DispatchEvent(GameEventGlobalDefine.ExitGamePage, null, null);
        Manager.GameManager.Instance.ReStart(false);
    }

    public void UnregistEvent()
    {
        Btn_Collect.onClick.RemoveListener(OnBtn_CollectClicked);
        Btn_TryGetRevive.onClick.RemoveListener(OnBtn_TryGetReviveClicked);
        Btn_Close.onClick.RemoveListener(OnBtn_CloseClicked);

    }


}
