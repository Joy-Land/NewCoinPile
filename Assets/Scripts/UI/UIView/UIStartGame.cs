using Joyland.GamePlay;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using ThinRL.Core;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

public class UIStartGame : UIViewBase
{
    public Image Img_Bg;
    public Text Txt_StageDesc;
    public Text Txt_Tesss;
    public Text Txt_Test;
    public Image Img_Slider;


    private int m_TotalProgress = 1;
    public override void OnViewAwake(EventArgsPack args)
    {
        base.OnViewAwake(args);

        Img_Bg = transform.Find("Img_Bg").GetComponent<Image>();
        Txt_StageDesc = transform.Find("Txt_StageDesc").GetComponent<Text>();
        Txt_Tesss = transform.Find("Txt_StageDesc/Txt_Tesss").GetComponent<Text>();
        Txt_Test = transform.Find("Txt_Test").GetComponent<Text>();
        Img_Slider = transform.Find("Progress/Img_Slider").GetComponent<Image>();


        Img_Bg.GetComponent<RectTransform>().offsetMin = -UIManager.Instance.FullOffset.offsetMin;
        Img_Bg.GetComponent<RectTransform>().offsetMax = -UIManager.Instance.FullOffset.offsetMax;

#if UNITY_EDITOR

#else
#if !UNITY_EDITOR && UNITY_WX
        Txt_StageDesc.font = J.Minigame.SystemFont;
#endif
#endif
        m_TotalProgress = args.GetData<int>(0);
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
            if(stageCode == (int)LoadingStageEventCode.Finish)
            {
                //可以发消息切状态机,关闭这个ui，正式进入游戏了
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
