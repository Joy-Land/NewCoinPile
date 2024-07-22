using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ThinRL.ProfilerHub.OverdrawMonitor
{
    [System.Serializable]
    public class CDOverdrawMonitor
    {
        [LabelText("请求计算Overdraw队列最大长度")]
        [Range(4, 32f)]
        public int maxRequestQueueLength = 8;// 异步获取数据请求队列的最长长度，队列满了后不在建立新的请求，会导致不在计算overdraw，直到队列有空出来
        [LabelText("Overdraw平均值采样时间")]
        [Range(0.01f, 5.0f)]
        public float sampleTime = 1.0f;
    }

    [System.Serializable]
    public class CDParticleMonitor
    {
        [LabelText("统计粒子采样时间")]
        [Range(0.01f, 5.0f)]
        public float sampleInterval = 0.5f;
    }
    /////////////////////////////////
    [System.Serializable]
    public class CDChartAppearance
    {
        [LabelText("背景透明度")]
        [Range(0.0f, 1.0f)]
        public float backroungTransparent = 0.25f;
        [LabelText("图表透明度")]
        [Range(0.0f, 1.0f)]
        public float chartTransparent = 1.0f;
    }
    [System.Serializable]
    public class CDChart
    {
        [LabelText("中消耗值")]
        public float middlingValue = 5.0f;
        [LabelText("高消耗值")]
        public float highValue = 10.0f;
        [LabelText("极高消耗值")]
        public float veryHighValue = 18.0f;
        [LabelText("消耗最大值")]
        public float fullValue = 10.0f;
    }
    
    [System.Serializable]
    public class CDOverdrawChart
    {
        [LabelText("目标相机")]
        public string cameraName = "";
        [LabelText("消耗等级阈值")]
        public CDChart chart = null;
        [LabelText("极高消耗帧数量阈值")]
        public int veryHighFrameCountThresholdOverdraw = 30;
    }
    [System.Serializable]
    public class CDOverdrawSummaryChart
    {
        [LabelText("消耗等级阈值")]
        public CDChart chart = null;
    }
    ///
    [System.Serializable]
    public class CDParticleChart
    {
        [LabelText("消耗等级阈值")]
        public CDChart chart = null;
    }
    ////////////////////////////////////////////


    [System.Serializable]
    public class ConfigDefine
    {
        [LabelText("UI根Canvas路径")]
        [Tooltip("screenspace的ui只能由一个相机渲染，需要识别出来特殊处理才能统计到overdraw")]
        public List<string> rootUICanvasPath = new List<string>();

        [LabelText("文字大小")]
        [Range(0.01f, 0.05f)]
        public float fontSize = 0.014f;

        [LabelText("图表大小")]
        [Range(0.1f, 0.5f)]
        public float chartSize = 0.15f;

        [LabelText("图表更新间隔(秒)")]
        [Range(0.02f, 0.2f)]
        public float chartUpdateInterval = 0.1f;

        [LabelText("强制每帧刷新图表(调试用)")]
        public bool forceUpdateEveryFrame = false;

        [PropertySpace(5.0f)]
        [LabelText("Overdraw监视器")]
        public CDOverdrawMonitor overdrawMonitor = null;

        [PropertySpace(5.0f)]
        [LabelText("粒子监视器")]
        public CDParticleMonitor particleMonitor = null;

        [PropertySpace(25.0f)]
        [LabelText("图表外观")]
        public CDChartAppearance chartAppearance = null;        

        [PropertySpace(25.0f)]
        [LabelText("粒子图表")]        
        public CDParticleChart particleChart = null;

        [PropertySpace(5.0f)]
        [LabelText("Overdraw汇总图表")]
        public CDOverdrawSummaryChart overdrawSummaryChart = null;

        [PropertySpace(5.0f)]
        [LabelText("Overdraw图表(默认)")]
        [InfoBox("这里目标相机不用填写，写了也没用\n自定义里找不到就使用默认")]
        public CDOverdrawChart overdrawChartDefault = null;

        [PropertySpace(5.0f)]
        [LabelText("Overdraw图表(自定义)")]
        [ListDrawerSettings()]
        public List<CDOverdrawChart> overdrawChartCustom = new List<CDOverdrawChart>();
    }
}