using DG.Tweening;
using Joyland.GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

public class UIHomePage : UIViewBase
{
    private int _UIOrder = 101;
    public override int UIOrder
    {
        get { return _UIOrder; }
        set { _UIOrder = value; }
    }

    public Image Img_Bg;
    public Image Img_House;
    public Image Img_Bord;
    public Text Txt_InterestRate;
    public Button Btn_StartGame;
    public Button Btn_Rank;
    public Button Btn_Collect;
    public Button Btn_Setting;
    public Button Btn_Bank;

    public Image Img_TestCoin;
    public Image Img_Cloud1;
    public Image Img_Cloud2;

    public Image Img_Slot;
    public RectTransform CoinsNode;

    private List<Image> m_CoinsList;
    private List<Image> m_CloudList;
    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);
        Img_Bg = transform.Find("Img_Bg").GetComponent<Image>();
        Img_Cloud1 = transform.Find("Img_Bg/Img_Cloud1").GetComponent<Image>();
        Img_Cloud2 = transform.Find("Img_Bg/Img_Cloud2").GetComponent<Image>();
        Img_House = transform.Find("Img_Bg/Img_House").GetComponent<Image>();
        Img_Bord = transform.Find("Img_Bg/Img_Bord").GetComponent<Image>();
        Txt_InterestRate = transform.Find("Img_Bg/Img_Bord/Txt_InterestRate").GetComponent<Text>();
        Btn_StartGame = transform.Find("Btn_StartGame").GetComponent<Button>();
        Btn_Rank = transform.Find("Btn_Rank").GetComponent<Button>();
        Btn_Collect = transform.Find("Btn_Collect").GetComponent<Button>();
        Btn_Setting = transform.Find("Btn_Setting").GetComponent<Button>();
        Btn_Bank = transform.Find("Btn_Bank").GetComponent<Button>();
        
        Img_Slot = transform.Find("Img_Bg/Image/AnimationNode/Img_Slot").GetComponent<Image>();
        Img_TestCoin = transform.Find("Img_Bg/Img_TestCoin").GetComponent<Image>();
        CoinsNode = transform.Find("Img_Bg/Image/AnimationNode/Img_Slot/CoinsNode").GetComponent<RectTransform>();

        Img_Bg.GetComponent<RectTransform>().offsetMin = -UIManager.Instance.FullOffset.offsetMin;
        Img_Bg.GetComponent<RectTransform>().offsetMax = -UIManager.Instance.FullOffset.offsetMax;

        m_CoinsList = CoinsNode.GetComponentsInChildren<Image>().ToList();

        m_CloudList = new List<Image>
        {
            Img_Cloud1,
            Img_Cloud2
        };
    }

    private Sequence m_AnimationSeq;
    public override void OnViewShow(EventArgsPack args)
    {
        base.OnViewShow(args);
        RegistEvent();

        m_AnimationSeq = DOTween.Sequence();
        //持续做动画硬币飞入的动画
        var sP = RectTransformUtility.WorldToScreenPoint(UIManager.Instance.UICamera, Img_TestCoin.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Img_Slot.rectTransform, sP, UIManager.Instance.UICamera, out var pos);

        console.error("len:", m_CoinsList.Count);
        for (int i = 0; i < m_CoinsList.Count; i++)
        {
            Sequence s = DOTween.Sequence();

            //定义一共2秒的 x 轴移动
            s.Append(m_CoinsList[i].rectTransform.DOAnchorPosX(pos.x, 1.3f).SetEase(Ease.InSine));

            //s.Insert(0, m_CoinsList[i].transform.DOScale(1.15f, 0.6f).SetEase(Ease.OutCubic));
            //s.Insert(0.6f, m_CoinsList[i].transform.DOScale(0.75f, 0.5f).SetEase(Ease.InCubic));

            s.Insert(0, m_CoinsList[i].rectTransform.DOAnchorPosY(pos.y, 1.3F).SetEase(Ease.OutCubic));
            //定义0 - 1秒的 y 轴移动
            //s.Insert(0, m_CoinsList[i].rectTransform.DOAnchorPosY(pos.y - 100, 0.6F).SetEase(Ease.OutCubic));


            //下落 1 - 2秒的 y 轴移动
            //s.Insert(0.6F, m_CoinsList[i].rectTransform.DOAnchorPosY(pos.y, 0.5F).SetEase(Ease.InCubic)).OnUpdate(() =>
            //{
            //});

            //播放
            //s.Play();
            //该动画组的回调方法
            s.OnComplete(() =>
            {
                //eff.SetActive(false);
            });
            s.SetDelay(i*0.1f);
            m_AnimationSeq.Join(s);
        }

        m_AnimationSeq.Play();
    }

    public Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
    {
        var mid = Vector2.Lerp(start, end, t);

        return new Vector2(mid.x, fff(t, height) + Mathf.Lerp(start.y, end.y, t));
    }

    float fff(float x, float height)
    {
        return 4 * (-height * x * x + height * x);
    }


    public override void OnViewUpdate()
    {
        base.OnViewUpdate();

        var len = m_CloudList.Count;
        for (int i = 0; i < len; i++)
        {
            var cloud = m_CloudList[i];
            cloud.rectTransform.anchoredPosition = new Vector2(cloud.rectTransform.anchoredPosition.x - 15 * Time.deltaTime, cloud.rectTransform.anchoredPosition.y);
            if(cloud.rectTransform.anchoredPosition.x < -(UIManager.Instance.FullSizeDetail.x * 0.5f + cloud.rectTransform.sizeDelta.x * 1.5))
            {
                cloud.rectTransform.anchoredPosition = new Vector2((UIManager.Instance.FullSizeDetail.x * 0.5f + 50), cloud.rectTransform.anchoredPosition.y);
            }
        }






        //Img_House.rectTransform.position
        //console.error(pos);
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
        Btn_StartGame.onClick.AddListener(OnBtn_StartGameClicked);
        Btn_Rank.onClick.AddListener(OnBtn_RankClicked);
        Btn_Collect.onClick.AddListener(OnBtn_CollectClicked);
        Btn_Setting.onClick.AddListener(OnBtn_SettingClicked);
        Btn_Bank.onClick.AddListener(OnBtn_BankClicked);

    }

    public RectTransform ss;
    public RectTransform ee;

    public void OnBtn_StartGameClicked()
    {
        EventManager.Instance.DispatchEvent(GameEventGlobalDefine.EnterGamePage, null, null);
        //YooAssets.LoadAssetAsync("Effect_Appear").Completed += (handle) =>
        //{
        //    var p = handle.AssetObject;
        //    console.error(p.name);
        //    var go = handle.InstantiateSync(Btn_StartGame.transform, true);
        //    go.transform.localPosition = new Vector3(0, 10, 10);
        //};
        //UIFlyElementSys.Instance.PlayFlyCoinEffect(ss.position, ee.position);
        //args[0]：是否带图片类型， args[1]：desc， args[2]：动画播放完成回调 args[3]：点击遮罩关闭回调
        //UIManager.Instance.OpenUI(UIViewID.UITips, new EventArgsPack(true, "你好，你的利息很高很高",
        //    new Action(() => { console.info("动画播放回调", this.name); }),
        //    new Action(() => { console.info("点击后的回调"); })));
    }


    public void OnBtn_RankClicked()
    {

    }

    public void OnBtn_CollectClicked()
    {
        UIManager.Instance.OpenUI(UIViewID.UICollect);
    }

    public void OnBtn_SettingClicked()
    {
        UIManager.Instance.OpenUI(UIViewID.UISetting, new EventArgsPack(false));
    }

    public void OnBtn_BankClicked()
    {
        UIManager.Instance.OpenUI(UIViewID.UIBank);
    }

    public void UnregistEvent()
    {
        Btn_StartGame.onClick.RemoveListener(OnBtn_StartGameClicked);
        Btn_Rank.onClick.RemoveListener(OnBtn_RankClicked);
        Btn_Collect.onClick.RemoveListener(OnBtn_CollectClicked);
        Btn_Setting.onClick.RemoveListener(OnBtn_SettingClicked);
        Btn_Bank.onClick.RemoveListener(OnBtn_BankClicked);

    }



}
