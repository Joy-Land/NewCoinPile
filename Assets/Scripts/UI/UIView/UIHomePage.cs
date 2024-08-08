using DG.Tweening;
using Joyland.GamePlay;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.UI;
using WeChatWASM;
using YooAsset;
using static Joyland.GamePlay.CommonUtil;

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

    private List<Vector2> m_CoinsPosList;
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
        m_CoinsPosList = new List<Vector2>();
        for(int i = 0; i < m_CoinsList.Count; i++)
        {
            m_CoinsPosList.Add(m_CoinsList[i].rectTransform.anchoredPosition);
        }
        m_CloudList = new List<Image>
        {
            Img_Cloud1,
            Img_Cloud2
        };
    }

    private Sequence m_AnimationSeq;

    private Sequence m_SlotSeq;
    public override void OnViewShow(EventArgsPack args)
    {
        base.OnViewShow(args);
        RegistEvent();

        if(m_SlotSeq != null)
        {
            m_SlotSeq.Kill();
            m_SlotSeq = null;
        }
        m_SlotSeq = DOTween.Sequence();
        var startX = -UIManager.Instance.FullSizeDetail.x * 0.5f - 600;
        var middleX = -100;
        var endX = UIManager.Instance.FullSizeDetail.x * 0.5f + 600;
        Img_Slot.rectTransform.anchoredPosition = new Vector2(startX, Img_Slot.rectTransform.anchoredPosition.y);


        m_SlotSeq.Insert(0,Img_Slot.rectTransform.DOAnchorPosX(middleX, 0.8f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            DoCoinsAnima();
        }));
        m_SlotSeq.Insert(3.5f, Img_Slot.rectTransform.DOAnchorPosX(endX, 0.8f).SetEase(Ease.InBack).OnComplete(() =>
        {
            ResetCoins();
        }));

        m_SlotSeq.SetLoops(-1);
    }

    void ResetCoins()
    {
        var len = m_CoinsList.Count;
        for (int i = 0; i < len; i++)
        {
            var sp = m_CoinsList[i];
            //显示硬币
            sp.gameObject.SetActive(true);
            //重置image
            sp.SetImageWithAsync("ui_home", "img_coin1");
            sp.SetNativeSize();
            //重置位置
            sp.rectTransform.anchoredPosition = m_CoinsPosList[i];
        }


    }

    void DoCoinsAnima()
    {
        if (m_AnimationSeq != null)
        {
            m_AnimationSeq.Kill();
            m_AnimationSeq = null;
        }
        m_AnimationSeq = DOTween.Sequence();
        //持续做动画硬币飞入的动画
        var sP = RectTransformUtility.WorldToScreenPoint(UIManager.Instance.UICamera, Img_TestCoin.transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Img_Slot.rectTransform, sP, UIManager.Instance.UICamera, out var pos);

        var idx = 0;
        for (int i = m_CoinsList.Count - 1; i >= 0; i--)
        {
            Sequence s = DOTween.Sequence();
            //console.error(m_CoinsList[i].name);

            var sp = m_CoinsList[i];
            //定义一共2秒的 x 轴移动
            s.Append(sp.rectTransform.DOAnchorPosX(pos.x, 0.9f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                sp.gameObject.SetActive(false);
            }));

            s.Insert(0, m_CoinsList[i].transform.DOScale(1.05f, 0.4f).SetEase(Ease.Linear));
            s.Insert(0.4f, m_CoinsList[i].transform.DOScale(0.65f, 0.4f).SetEase(Ease.Linear));



            s.Insert(0, DOVirtual.Float(0, 1, 0.12f, (e) => { }).OnComplete(() =>
            {
                sp.SetImageWithAsync("ui_home", "img_coin2");
                sp.SetNativeSize();
            }));
            s.Insert(0.12f, DOVirtual.Float(0, 1, 0.18f, (e) => { }).OnComplete(() =>
            {
                sp.SetImageWithAsync("ui_home", "img_coin3");
                sp.SetNativeSize();
            }));
            s.Insert(0.3f, DOVirtual.Float(0, 1, 0.18f, (e) => { }).OnComplete(() =>
            {
                sp.SetImageWithAsync("ui_home", "img_coin4");
                sp.SetNativeSize();
            }));

            s.Insert(0, sp.rectTransform.DOAnchorPosY(pos.y, 0.9F).SetEase(Ease.OutCubic));
            //定义0 - 1秒的 y 轴移动
            //s.Insert(0, m_CoinsList[i].rectTransform.DOAnchorPosY(pos.y - 100, 0.6F).SetEase(Ease.OutCubic));


            //下落 1 - 2秒的 y 轴移动
            //s.Insert(0.6F, m_CoinsList[i].rectTransform.DOAnchorPosY(pos.y, 0.5F).SetEase(Ease.InCubic)).OnUpdate(() =>
            //{
            //});

            //播放
            //s.Play();
            //该动画组的回调方法
            s.SetDelay(idx * 0.1f);
            m_AnimationSeq.Join(s);
            idx++;
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

        if (m_AnimationSeq != null)
        {
            m_AnimationSeq.Kill();
            m_AnimationSeq = null;
        }
        if (m_SlotSeq != null)
        {
            m_SlotSeq.Kill();
            m_SlotSeq = null;
        }
    }

    public void RegistEvent()
    {
        Btn_StartGame.onClick.AddListener(OnBtn_StartGameClicked);
        Btn_Rank.onClick.AddListener(OnBtn_RankClicked);
        Btn_Collect.onClick.AddListener(OnBtn_CollectClicked);
        Btn_Setting.onClick.AddListener(OnBtn_SettingClicked);
        Btn_Bank.onClick.AddListener(OnBtn_BankClicked);


        EventManager.Instance.AddEvent(GameEventGlobalDefine.OnGetGameTools, OnGetGetToolsEvent);
    }

    public RectTransform ss;
    public RectTransform ee;


    public void StartGame()
    {
        EventManager.Instance.DispatchEvent(GameEventGlobalDefine.EnterGamePage, null, null);
    }

    public void OnBtn_StartGameClicked()
    {
        (ItemCategoryID categoryID, int remainNumber) = GameConfig.GameItemManager.GetItemCategoryData(ItemID.PlayGame);
        if (categoryID == ItemCategoryID.Default)
        {
            GameConfig.GameItemManager.UpdateItemCategoryData(ItemID.PlayGame, categoryID, -1);
            J.ReqHelper.UpdateItemData((int)ItemID.PlayGame, (int)categoryID, 0, remainNumber - 1, (resData) =>
            {
                GameConfig.Instance.SetItemData(resData);
            });
            StartGame();
        }
        else
        {
            UIManager.Instance.OpenUI(UIViewID.UIPopupItem, new EventArgsPack(ItemID.PlayGame));
        }
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

    public void OnGetGetToolsEvent(object sender, EventArgsPack e)
    {
        var itemId = (ItemID)e.GetData<ItemID>(0);

        if (itemId == ItemID.PlayGame)
        {
            StartGame();
        }
    }

    public void UnregistEvent()
    {
        Btn_StartGame.onClick.RemoveListener(OnBtn_StartGameClicked);
        Btn_Rank.onClick.RemoveListener(OnBtn_RankClicked);
        Btn_Collect.onClick.RemoveListener(OnBtn_CollectClicked);
        Btn_Setting.onClick.RemoveListener(OnBtn_SettingClicked);
        Btn_Bank.onClick.RemoveListener(OnBtn_BankClicked);

        EventManager.Instance.RemoveEvent(GameEventGlobalDefine.OnGetGameTools, OnGetGetToolsEvent);
    }



}
