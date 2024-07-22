using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace ThinRL.ProfilerHub.OverdrawMonitor
{
    public class ProfilingChartRendererOverdraw : ProfilingChartRenderer
    {
        CDOverdrawChart m_Config = null;
        OverdrawMonitor m_Monitor;
        string m_CameraName;
        string m_CameraNameSimplified;
        int m_veryHighFrameCount = 0;
        public static CDOverdrawChart FetchConfig(CDOverdrawChart configDefault, List<CDOverdrawChart> configCustoms, string cameraName)
        {
            CDOverdrawChart config = null;
            if (configCustoms != null)
            {
                foreach (CDOverdrawChart x in configCustoms)
                {
                    if (x.cameraName != cameraName) continue;
                    config = x;
                }
            }

            if (config == null)
            {
                config = configDefault;
            }

            return config;
        }
        public ProfilingChartRendererOverdraw(string name, CDOverdrawChart config, CDChartAppearance configAppearance, OverdrawMonitor monitor) : base(name, config.chart, configAppearance)
        {
            m_Monitor = monitor;
            m_CameraName = (m_Monitor != null) ? m_Monitor.GetTargetCameraName() : "Unknow";
            m_Config = config;
            //如果格式是xxxCamera的话，截取前面一段；如果index == 0,不动；找不到Camera也不动。
            int index = m_CameraName.ToLower().IndexOf("camera");
            m_CameraNameSimplified = (index > 0) ? m_CameraName.Substring(0, index) : m_CameraName;
        }
        public override void UninitializeRenderer()
        {
            m_Monitor = null;
            base.UninitializeRenderer();
        }

        public override void DoUpdateFrame()
        {
            float ratio = (m_Monitor != null) ? m_Monitor.OverdrawRatio : 0.0f;
            float levelValue = m_Config.chart.veryHighValue;
            if (ratio >= levelValue)
            {
                m_veryHighFrameCount++;
            }
        }
        public override void DoRecord()
        {
            if (m_Monitor == null) return;
            float recordValue = m_Monitor.OverdrawRatio;
            RecordChartValue(recordValue);
        }
        Color m_ColorForOptimization = Color.white;
        GUIStyle m_TextStyle = null;
        GUIStyle m_ChartStyle = null;
        public override void DoGUI(bool foldout)
        {
            CalculateSizes(foldout, out int fontSize, out int chartWidth, out int chartHeight);

            if (null == m_TextStyle)
            {
                m_TextStyle = new GUIStyle("Label") { normal = new GUIStyleState() };
            }
            if (null == m_ChartStyle)
            {
                m_ChartStyle = new GUIStyle("Label") { normal = new GUIStyleState() };
            }
            m_TextStyle.fontSize = fontSize;
            m_TextStyle.fontStyle = FontStyle.Bold;
            m_ChartStyle.alignment = TextAnchor.MiddleCenter;
            float labelHeight = m_TextStyle.lineHeight + m_TextStyle.margin.bottom;
            GUILayoutOption layoutOption = GUILayout.Height(labelHeight);

            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                if (!foldout)
                {
                    string text = GetCameraText();
                    m_TextStyle.normal.textColor = Color.white;
                    GUILayout.Label(text, m_TextStyle, layoutOption);

                    GetVeryHighFrameCountTextParams(ref text, ref m_ColorForOptimization);
                    m_TextStyle.normal.textColor = m_ColorForOptimization;
                    GUILayout.Label(text, m_TextStyle, layoutOption);
                }
                GUILayout.EndHorizontal();

                if (!foldout)
                {
                    if (m_RenderTextureChart != null)
                    {
                        GUILayout.Label(m_RenderTextureChart, m_ChartStyle, GUILayout.Width(chartWidth), GUILayout.Height(chartHeight));
                    }
                }

                if (foldout)
                {
                    string text = GetRichtextString();
                    m_TextStyle.normal.textColor = Color.white;
                    GUILayout.Label(text, m_TextStyle, layoutOption);
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    {
                        string text = "";
                        GetCurTextParams(ref text, ref m_ColorForOptimization);
                        m_TextStyle.normal.textColor = m_ColorForOptimization;
                        GUILayout.Label(text, m_TextStyle, layoutOption);
                    }
                    {
                        string text = "";
                        GetCurPeakValueTextParams(ref text, ref m_ColorForOptimization);
                        m_TextStyle.normal.textColor = m_ColorForOptimization;
                        GUILayout.Label(text, m_TextStyle, layoutOption);
                    }
                    {
                        string text = "";
                        GetHistoryPeakValueTextParams(ref text, ref m_ColorForOptimization);
                        m_TextStyle.normal.textColor = m_ColorForOptimization;
                        GUILayout.Label(text, m_TextStyle, layoutOption);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }
        StringBuilder m_TextBuilder = null;
        string GetCameraText()
        {
            if (m_TextBuilder == null)
            {
                m_TextBuilder = new StringBuilder(512);
            }

            m_TextBuilder.Clear();
            string cameraName = m_CameraName;
            m_TextBuilder.AppendLine($"{cameraName}");
            return m_TextBuilder.ToString();

        }
        void GetVeryHighFrameCountTextParams(ref string text, ref Color textColor)
        {
            m_TextBuilder.Clear();
            int frameCount = m_veryHighFrameCount;
            m_TextBuilder.AppendLine($"极高消耗帧数：{frameCount}");
            text = m_TextBuilder.ToString();

            if (frameCount >= m_Config.veryHighFrameCountThresholdOverdraw)
            {
                textColor = s_VeryHighColor;
            }
            else
            {
                textColor = s_LowColor;
            }
            textColor.a = 1.0f;
        }
        string GetRichtextString()
        {
            if (m_TextBuilder == null)
            {
                m_TextBuilder = new StringBuilder(512);
            }

            m_TextBuilder.Clear();

            string cameraName = m_CameraNameSimplified;

            float ratio = (m_Monitor != null) ? m_Monitor.OverdrawRatio : 0.0f;
            float curPeakValue = maxRecordedPeakValue;
            float historyPeakValue = (m_Monitor != null) ? m_Monitor.MaxOverdraw : 0.0f;

            string colorRatioRichext = GetLevelRichtextColorString(ratio);
            string colorCurPeakValueRichext = GetLevelRichtextColorString(curPeakValue);
            string colorHistoryPeakValueRichext = GetLevelRichtextColorString(historyPeakValue);

            m_TextBuilder.AppendLine($"{cameraName}:" +
                $"<color={colorRatioRichext}>{ratio:F1}</color>" +
                $"/<color={colorCurPeakValueRichext}>{curPeakValue:F1}</color>" +
                $"/<color={colorHistoryPeakValueRichext}>{historyPeakValue:F1}</color>");

            return m_TextBuilder.ToString();
        }
        void GetCurTextParams(ref string text, ref Color textColor)
        {
            if (m_TextBuilder == null)
            {
                m_TextBuilder = new StringBuilder(512);
            }

            m_TextBuilder.Clear();
            float ratio = (m_Monitor != null) ? m_Monitor.OverdrawRatio : 0.0f;
            m_TextBuilder.AppendLine($"当前：{ratio:F1}");
            text = m_TextBuilder.ToString();

            GetLevelColor(ratio, ref textColor);
        }
        void GetCurPeakValueTextParams(ref string text, ref Color textColor)
        {
            if (m_TextBuilder == null)
            {
                m_TextBuilder = new StringBuilder(512);
            }

            m_TextBuilder.Clear();
            float curPeakValue = maxRecordedPeakValue;
            m_TextBuilder.AppendLine($"峰值：{curPeakValue:F1}");
            text = m_TextBuilder.ToString();

            GetLevelColor(curPeakValue, ref textColor);
        }
        void GetHistoryPeakValueTextParams(ref string text, ref Color textColor)
        {
            if (m_TextBuilder == null)
            {
                m_TextBuilder = new StringBuilder(512);
            }

            m_TextBuilder.Clear();
            float historyPeakValue = (m_Monitor != null) ? m_Monitor.MaxOverdraw : 0.0f;
            m_TextBuilder.AppendLine($"历史峰值：{historyPeakValue:F1}");
            text = m_TextBuilder.ToString();

            GetLevelColor(historyPeakValue, ref textColor);
        }
    }
    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    public class ProfilingChartRendererOverdrawSummary : ProfilingChartRenderer
    {
        List<OverdrawMonitor> m_Monitors;//引用外部，只读，不能操作
        public ProfilingChartRendererOverdrawSummary(string name, CDOverdrawSummaryChart config, CDChartAppearance configAppearance, List<OverdrawMonitor> monitors) : base(name, config.chart, configAppearance)
        {
            m_Monitors = monitors;
        }
        public override void UninitializeRenderer()
        {
            m_Monitors = null;
            base.UninitializeRenderer();
        }

        public override void DoUpdateFrame()
        {
        }
        public override void DoRecord()
        {
            if (m_Monitors == null) return;
            float recordValue = 0.0f;
            foreach (OverdrawMonitor x in m_Monitors)
            {
                recordValue += x.OverdrawRatio;
            }
            RecordChartValue(recordValue);
        }

        GUIStyle m_ChartStyle = null;
        public override void DoGUI(bool foldout)
        {
            CalculateSizes(foldout, out int fontSize, out int chartWidth, out int chartHeight);

            if (null == m_ChartStyle)
            {
                m_ChartStyle = new GUIStyle("Label") { normal = new GUIStyleState() };
            }
            m_ChartStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.BeginVertical();
            {
                if (foldout)
                {
                    if (m_RenderTextureChart != null)
                    {
                        GUILayout.Label(m_RenderTextureChart, m_ChartStyle, GUILayout.Width(chartWidth), GUILayout.Height(chartHeight));
                    }
                }                
            }
            GUILayout.EndVertical();
        }
    }
    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    public class ProfilingChartRendererParticle : ProfilingChartRenderer
    {
        ParticleMonitor m_Monitor;
        public ProfilingChartRendererParticle(string name, CDParticleChart config, CDChartAppearance configAppearance, ParticleMonitor monitor) : base(name, config.chart, configAppearance)
        {
            m_Monitor = monitor;
        }
        public override void UninitializeRenderer()
        {
            m_Monitor = null;
            base.UninitializeRenderer();
        }
        public override void DoUpdateFrame()
        {

        }
        public override void DoRecord()
        {
            if (m_Monitor == null) return;
            float recordValue = m_Monitor.particleCount;
            RecordChartValue(recordValue);
        }
        Color m_ColorForOptimization = Color.white;
        GUIStyle m_TextStyle = null;
        GUIStyle m_ChartStyle = null;

        public override void DoGUI(bool foldout)
        {
            CalculateSizes(foldout, out int fontSize, out int chartWidth, out int chartHeight);

            if (null == m_TextStyle)
            {
                m_TextStyle = new GUIStyle("Label") { normal = new GUIStyleState() };
            }
            if (null == m_ChartStyle)
            {
                m_ChartStyle = new GUIStyle("Label") { normal = new GUIStyleState() };
            }
            m_TextStyle.fontSize = fontSize;
            m_TextStyle.fontStyle = FontStyle.Bold;
            m_ChartStyle.alignment = TextAnchor.MiddleCenter;

            float labelHeight = m_TextStyle.lineHeight + m_TextStyle.margin.bottom;
            GUILayoutOption layoutOption = GUILayout.Height(labelHeight);

            GUILayout.BeginVertical();
            {
                if (!foldout)
                {
                    GUILayout.BeginHorizontal();
                    {
                        string text = "粒子";
                        m_TextStyle.normal.textColor = Color.white;
                        GUILayout.Label(text, m_TextStyle, layoutOption);
                    }
                    {
                        string text = GetParticleSystemNotInPlayingCountString(false);
                        m_TextStyle.normal.textColor = Color.white;
                        GUILayout.Label(text, m_TextStyle, layoutOption);
                    }
                    GUILayout.EndHorizontal();

                }
                if (m_RenderTextureChart != null)
                {
                    GUILayout.Label(m_RenderTextureChart, m_ChartStyle, GUILayout.Width(chartWidth), GUILayout.Height(chartHeight));
                }

                if (foldout)
                {
                    {
                        string text = GetRichtextString();
                        m_TextStyle.normal.textColor = Color.white;
                        GUILayout.Label(text, m_TextStyle, layoutOption);
                    }
                    {
                        string text = GetParticleSystemNotInPlayingCountString(true);
                        m_TextStyle.normal.textColor = Color.white;
                        GUILayout.Label(text, m_TextStyle, layoutOption);
                    }
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    {
                        string text = "";
                        GetCurTextParams(ref text, ref m_ColorForOptimization);
                        m_TextStyle.normal.textColor = m_ColorForOptimization;
                        GUILayout.Label(text, m_TextStyle, layoutOption);
                    }
                    {
                        string text = "";
                        GetCurPeakValueTextParams(ref text, ref m_ColorForOptimization);
                        m_TextStyle.normal.textColor = m_ColorForOptimization;
                        GUILayout.Label(text, m_TextStyle, layoutOption);
                    }
                    {
                        string text = "";
                        GetHistoryPeakValueTextParams(ref text, ref m_ColorForOptimization);
                        m_TextStyle.normal.textColor = m_ColorForOptimization;
                        GUILayout.Label(text, m_TextStyle, layoutOption);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }

        StringBuilder m_TextBuilder = null;
        string GetRichtextString()
        {
            if (m_TextBuilder == null)
            {
                m_TextBuilder = new StringBuilder(512);
            }

            m_TextBuilder.Clear();

            int count = (m_Monitor != null) ? m_Monitor.particleCount : 0;
            int curPeakValue = (int)maxRecordedPeakValue;
            int historyPeakValue = (m_Monitor != null) ? m_Monitor.particlePeakValue : 0;

            string colorCountRichext = GetLevelRichtextColorString(count);
            string colorCurPeakValueRichext = GetLevelRichtextColorString(curPeakValue);
            string colorHistoryPeakValueRichext = GetLevelRichtextColorString(historyPeakValue);

            m_TextBuilder.AppendLine($"粒子:" +
                $"<color={colorCountRichext}>{count}</color>" +
                $"/<color={colorCurPeakValueRichext}>{curPeakValue}</color>" +
                $"/<color={colorHistoryPeakValueRichext}>{historyPeakValue}</color>");

            return m_TextBuilder.ToString();
        }
        void GetCurTextParams(ref string text, ref Color textColor)
        {
            if (m_TextBuilder == null)
            {
                m_TextBuilder = new StringBuilder(512);
            }

            m_TextBuilder.Clear();
            int count = (m_Monitor != null) ? m_Monitor.particleCount : 0;
            m_TextBuilder.AppendLine($"当前：{count}");
            text = m_TextBuilder.ToString();

            GetLevelColor(count, ref textColor);
        }
        void GetCurPeakValueTextParams(ref string text, ref Color textColor)
        {
            if (m_TextBuilder == null)
            {
                m_TextBuilder = new StringBuilder(512);
            }

            m_TextBuilder.Clear();
            int curPeakValue = (int)maxRecordedPeakValue;
            m_TextBuilder.AppendLine($"峰值：{curPeakValue}");
            text = m_TextBuilder.ToString();

            GetLevelColor(curPeakValue, ref textColor);
        }
        void GetHistoryPeakValueTextParams(ref string text, ref Color textColor)
        {
            if (m_TextBuilder == null)
            {
                m_TextBuilder = new StringBuilder(512);
            }

            m_TextBuilder.Clear();
            int count = (m_Monitor != null) ? m_Monitor.particlePeakValue : 0;
            m_TextBuilder.AppendLine($"历史峰值：{count}");
            text = m_TextBuilder.ToString();

            GetLevelColor(count, ref textColor);
        }
        string GetParticleSystemNotInPlayingCountString(bool foldout)
        {
            if (m_TextBuilder == null)
            {
                m_TextBuilder = new StringBuilder(512);
            }
            m_TextBuilder.Clear();
            int countPlaying = (m_Monitor != null) ? m_Monitor.particleSystemCountPlaying : 0;
            int countNotPlaying = (m_Monitor != null) ? m_Monitor.particleSystemCountNotPlaying : 0;
            int countPrefab = (m_Monitor != null) ? m_Monitor.particleSystemCountPrefab : 0;
            if (foldout)
            {
                m_TextBuilder.AppendLine($"PS:{countPlaying}/{countNotPlaying}/{countPrefab}");
            }
            else
            {
                m_TextBuilder.AppendLine($"播:{countPlaying} 停:{countNotPlaying} Prefab:{countPrefab}");
            }
            return m_TextBuilder.ToString();
        }
    }
}
