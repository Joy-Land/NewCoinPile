using DG.Tweening;
using Joyland.GamePlay;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using WeChatWASM;


public class UIStartGame : UIViewBase
{
    public Image Img_Bg;
    public Text Txt_StageDesc;
    public Text Txt_Tesss;
    public Text Txt_Test;
    public Image Img_Slider;

    public Image Img_Coin;

    public Image emptyStart;
    public Image yi;
    public Image ge;
    public Image xiao;
    public Image mu;
    public Image biao;

    private int m_TotalProgress = 1;
    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);

        Img_Bg = transform.Find("Img_Bg").GetComponent<Image>();
        Txt_StageDesc = transform.Find("Txt_StageDesc").GetComponent<Text>();
        Txt_Tesss = transform.Find("Txt_StageDesc/Txt_Tesss").GetComponent<Text>();
        Txt_Test = transform.Find("Txt_Test").GetComponent<Text>();
        Img_Slider = transform.Find("Progress/Img_Slider").GetComponent<Image>();
        Img_Coin = transform.Find("AnimateNode/Img_Coin").GetComponent<Image>();

        emptyStart = transform.Find("AnimateNode/emptyStart").GetComponent<Image>();
        yi = transform.Find("AnimateNode/yi").GetComponent<Image>();
        ge = transform.Find("AnimateNode/ge").GetComponent<Image>();
        xiao = transform.Find("AnimateNode/xiao").GetComponent<Image>();
        mu = transform.Find("AnimateNode/mu").GetComponent<Image>();
        biao = transform.Find("AnimateNode/biao").GetComponent<Image>();


        Img_Bg.GetComponent<RectTransform>().offsetMin = -UIManager.Instance.FullOffset.offsetMin;
        Img_Bg.GetComponent<RectTransform>().offsetMax = -UIManager.Instance.FullOffset.offsetMax;

        emptyStart.rectTransform.anchoredPosition = new Vector2(-Img_Bg.rectTransform.rect.width , emptyStart.rectTransform.anchoredPosition.y);
        console.warn(emptyStart.rectTransform.anchoredPosition,Img_Bg.rectTransform.rect);
        Img_Coin.rectTransform.anchoredPosition = emptyStart.rectTransform.anchoredPosition;

#if UNITY_EDITOR

#else
#if UNITY_WX
        var fallbackFont = Application.streamingAssetsPath + "fallback.ttf";
        WX.GetWXFont(fallbackFont, (font) =>
        {
            //m_SystemFont = font;
            Txt_StageDesc.font = font;
            console.error("fzy font:", font);
        });

        console.error("fzy zzz font:", J.Minigame.SystemFont);
#endif
#endif
        m_TotalProgress = args.GetData<int>(0);
    }

    private Sequence m_AnimationSeq = null;
    public override void OnViewShow(EventArgsPack args)
    {
        base.OnViewShow(args);
        RegistEvent();

        m_AnimationSeq = DOTween.Sequence();

        m_AnimationSeq.Append(DOTween.To(setter: value =>
        {
            Img_Coin.rectTransform.anchoredPosition = Parabola(emptyStart.rectTransform.anchoredPosition + new Vector2(0, emptyStart.rectTransform.sizeDelta.y) * 1.39f, yi.rectTransform.anchoredPosition + new Vector2(0, yi.rectTransform.sizeDelta.y) * 1.39f, 150, value);
        }, startValue: 0, endValue: 1, duration: 0.45f).SetEase(Ease.Linear).OnComplete(() => { TT(yi.rectTransform); }));

        m_AnimationSeq.Append(DOTween.To(setter: value =>
        {
            Img_Coin.rectTransform.anchoredPosition = Parabola(yi.rectTransform.anchoredPosition + new Vector2(0, yi.rectTransform.sizeDelta.y) * 1.39f, ge.rectTransform.anchoredPosition + new Vector2(0, ge.rectTransform.sizeDelta.y) * 1.39f, 180, value);
        }, startValue: 0, endValue: 1, duration: 0.35f).SetEase(Ease.Linear).OnComplete(() => { TT(ge.rectTransform); }));

        m_AnimationSeq.Append(DOTween.To(setter: value =>
        {
            Img_Coin.rectTransform.anchoredPosition = Parabola(ge.rectTransform.anchoredPosition + new Vector2(0, ge.rectTransform.sizeDelta.y) * 1.39f, xiao.rectTransform.anchoredPosition + new Vector2(0, xiao.rectTransform.sizeDelta.y) * 1.39f, 130, value);
        }, startValue: 0, endValue: 1, duration: 0.35f).SetEase(Ease.Linear).OnComplete(() => { TT(xiao.rectTransform); }));

        m_AnimationSeq.Append(DOTween.To(setter: value =>
        {
            Img_Coin.rectTransform.anchoredPosition = Parabola(xiao.rectTransform.anchoredPosition + new Vector2(0, xiao.rectTransform.sizeDelta.y) * 1.39f, mu.rectTransform.anchoredPosition + new Vector2(0, mu.rectTransform.sizeDelta.y) * 1.39f, 130, value);
        }, startValue: 0, endValue: 1, duration: 0.35f).SetEase(Ease.Linear).OnComplete(() => { TT(mu.rectTransform); }));

        m_AnimationSeq.Append(DOTween.To(setter: value =>
        {
            Img_Coin.rectTransform.anchoredPosition = Parabola(mu.rectTransform.anchoredPosition + new Vector2(0, mu.rectTransform.sizeDelta.y) * 1.39f, biao.rectTransform.anchoredPosition + new Vector2(0, biao.rectTransform.sizeDelta.y) * 1.39f, 130, value);
        }, startValue: 0, endValue: 1, duration: 0.35f).SetEase(Ease.Linear).OnComplete(() => { TT(biao.rectTransform, 0.2f, () => { if (m_AnimationSeq != null) { m_AnimationSeq.Kill(); m_AnimationSeq = null; } }); }));

    }

    public void TT(RectTransform target, float duration = 0.2f, Action cb = null)
    {
        // 创建一个 Sequence 对象
        Sequence mySequence = DOTween.Sequence();

        // 第一步：缩小 Y 轴到 0.5
        mySequence.Append(target.DOScaleY(0.7f, duration *0.4f)); // 持续时间为总时间的一半

        // 第二步：以 Bounce 缓动曲线放大 Y 轴到 1
        mySequence.Append(target.DOScaleY(1f, duration *0.6f).SetEase(Ease.OutBounce)); // 持续时间为总时间的一半

        mySequence.OnComplete(() =>
        {
            cb?.Invoke();
        });
        // 开始播放动画
        mySequence.Play();
    }


    public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
    {
        float Func(float x) => 4 * (-height * x * x + height * x);

        var mid = Vector2.Lerp(start, end, t);

        return new Vector2(mid.x, Func(t) + Mathf.Lerp(start.y, end.y, t));
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
        EventManager.Instance.AddEvent(GameEventGlobalDefine.AddProgressBar, OnAddProgressBarEvent);
    }


    private static readonly string[] m_StageDescTable = new string[]
    {
        "初始化中...",
        "正在检查及下载资源 (当前:{0}  共计:{1})",
        "初始化资源中...",
        "完成"
    };

    private void OnAddProgressBarEvent(object sender, EventArgsPack e)
    {
        var stageCode = e.GetData<int>(0);

        //阶段描述
        var stageDesc = m_StageDescTable[stageCode];
        //当前进度
        var curProgress = e.GetData<float>(1);
        //console.error("code", stageCode,curProgress,m_TotalProgress);
        //额外处理
        if (stageCode == (int)LoadingStageEventCode.DownloadPackage && e.ArgsLength >= 2)
        {
            var curDownlaod = e.GetData<int>(2);
            var totalDownload = e.GetData<int>(3);
            //Txt_StageDesc.text = stageDesc + curDownlaod.ToString()+"/"+totalDownload.ToString();
            Txt_StageDesc.text = string.Format(stageDesc, curDownlaod, totalDownload);
        }
        else
        {
            Txt_StageDesc.text = stageDesc;
            if (stageCode == (int)LoadingStageEventCode.Finish)
            {
                //可以发消息切状态机,关闭这个ui，正式进入游戏了
                if (m_AnimationSeq != null)
                {
                    m_AnimationSeq.Kill();
                    m_AnimationSeq = null;
                }
                EventManager.Instance.DispatchEvent(GameEventGlobalDefine.EverythingIsReady, null, null);
            }
        }
        curProgress = Mathf.Min(curProgress, (float)m_TotalProgress);
        Img_Slider.fillAmount = curProgress / m_TotalProgress;
    }



    public void UnregistEvent()
    {
        EventManager.Instance.RemoveEvent(GameEventGlobalDefine.AddProgressBar, OnAddProgressBarEvent);
    }


}
