using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
namespace ThinRL.ProfilerHub.OverdrawMonitor
{
    public abstract class ProfilingChartRenderer
    {
        private string m_Name;

        private CDChart m_Config = null;
        private CDChartAppearance m_ConfigAppearance = null;

        private const int k_MaxChartValuesLength = 128;//512;//equal of MAX_CHART_VALUES_LENGTH in Shader "ThinRL/Debug/ProfilingChart"
        private float[] m_ChartValues = new float[k_MaxChartValuesLength];
        private float[] m_RecordedChartValues = new float[k_MaxChartValuesLength];
        private int m_RecordedChartValueStartIndex = 0;
        private int m_RecordedChartValueValidLength = 0;

        private const int k_OnePeakValueRecordTimes = 10;
        private const int k_MaxRecordedPeakValueLength = k_MaxChartValuesLength / k_OnePeakValueRecordTimes + 1;
        private int m_PeakValueRecordTimes = 0;
        private float[] m_RecordedPeakValues = new float[k_MaxRecordedPeakValueLength];
        private int m_RecordedPeakValueCurrentIndex = 0;
        private float m_MaxRecordedPeakValue = 0.0f;
        protected float maxRecordedPeakValue { get { return m_MaxRecordedPeakValue; } }

        protected RenderTexture m_RenderTextureChart = null;
        private Material m_MaterialChart = null;

        protected static readonly Color s_LowColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
        protected static readonly Color s_MiddlingColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
        protected static readonly Color s_HighColor = new Color(1.0f, 0.5f, 0.0f, 1.0f);
        protected static readonly Color s_VeryHighColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);

        protected static readonly string s_RichtextLowColorString = ConvertColor32ToRichtextColor(s_LowColor);
        protected static readonly string s_RichtextMiddlingColorString = ConvertColor32ToRichtextColor(s_MiddlingColor);
        protected static readonly string s_RichtextHighColorString = ConvertColor32ToRichtextColor(s_HighColor);
        protected static readonly string s_RichtextVeryHighColorString = ConvertColor32ToRichtextColor(s_VeryHighColor);
        public ProfilingChartRenderer(string name, CDChart config, CDChartAppearance configAppearance)
        {
            m_Name = name;
            m_Config = config;
            m_ConfigAppearance = configAppearance;
        }
        public virtual void InitializeRenderer()
        {
            if (m_MaterialChart == null)
            {
                m_MaterialChart = new Material(Shader.Find("ThinRL/ProfilerHub/Debug/ProfilingChart"));//如果不在编辑器下运行，这里会找不到
            }
            if (m_RenderTextureChart == null)
            {
                m_RenderTextureChart = new RenderTexture(k_MaxChartValuesLength * 6, k_MaxChartValuesLength, 0);
                m_RenderTextureChart.useMipMap = false;
                m_RenderTextureChart.Create();
            }
            ClearChartValues();
        }
        public virtual void UninitializeRenderer()
        {
            m_MaterialChart = null;
            if (m_RenderTextureChart != null)
            {
                m_RenderTextureChart.Release();
                m_RenderTextureChart = null;
            }
        }
        public bool isSelfName(string name)
        {
            return (m_Name == name) ? true : false;
        }
        public void ChangeTargetWH(int targetWidth, int targetHight)
        {
            if (m_RenderTextureChart != null)
            {
                if (m_RenderTextureChart.width == targetWidth && m_RenderTextureChart.height == targetHight) return;

                m_RenderTextureChart.Release();
                m_RenderTextureChart = null;
            }
                        
            if (m_RenderTextureChart == null)
            {
                m_RenderTextureChart = new RenderTexture(targetWidth, targetHight, 0);
                m_RenderTextureChart.useMipMap = false;
                m_RenderTextureChart.Create();
            }
        }
        public void ClearChartValues()
        {
            System.Array.Clear(m_ChartValues, 0, m_ChartValues.Length);
            System.Array.Clear(m_RecordedChartValues, 0, m_RecordedChartValues.Length);
            m_RecordedChartValueStartIndex = 0;
            m_RecordedChartValueValidLength = 0;

            m_PeakValueRecordTimes = 0;
            System.Array.Clear(m_RecordedPeakValues, 0, m_RecordedPeakValues.Length);
            m_RecordedPeakValueCurrentIndex = 0;
            m_MaxRecordedPeakValue = 0.0f;
        }
        public void RecordChartValue(float v)
        {
            //记录图表值。利用定长数组，循环赋值
            {
                if (m_RecordedChartValueValidLength >= m_RecordedChartValues.Length)
                {
                    m_RecordedChartValues[m_RecordedChartValueStartIndex] = v;
                    ++m_RecordedChartValueStartIndex;
                    if (m_RecordedChartValueStartIndex >= m_RecordedChartValues.Length)
                    {
                        m_RecordedChartValueStartIndex = 0;
                    }
                }
                else
                {
                    m_RecordedChartValues[m_RecordedChartValueValidLength] = v;
                    ++m_RecordedChartValueValidLength;
                }
            }
            //统计峰值。利用定长数组，循环赋值。但这个数组不在乎起点和长度，单纯循环即可
            {
                //先记录一次，统计计算一次峰值
                float lv = m_RecordedPeakValues[m_RecordedPeakValueCurrentIndex];
                if (v > lv)
                {
                    m_RecordedPeakValues[m_RecordedPeakValueCurrentIndex] = v;
                    if (v > m_MaxRecordedPeakValue)
                    {
                        m_MaxRecordedPeakValue = v;
                    }
                }
                //超过一定记录次数，重新统计峰值。同时移动循环指针，准备下一次记录
                ++m_PeakValueRecordTimes;
                if (m_PeakValueRecordTimes >= k_OnePeakValueRecordTimes)
                {
                    m_PeakValueRecordTimes = 0;

                    {//统计最大值
                        m_MaxRecordedPeakValue = 0.0f;
                        for (int i = 0; i < k_MaxRecordedPeakValueLength; i++)
                        {
                            float pv = m_RecordedPeakValues[i];
                            if (pv > m_MaxRecordedPeakValue)
                            {
                                m_MaxRecordedPeakValue = pv;
                            }
                        }
                    }
                    //循环指针
                    ++m_RecordedPeakValueCurrentIndex;
                    if (m_RecordedPeakValueCurrentIndex >= k_MaxRecordedPeakValueLength)
                    {
                        m_RecordedPeakValueCurrentIndex = 0;
                    }
                    //准备下一次记录
                    m_RecordedPeakValues[m_RecordedPeakValueCurrentIndex] = 0.0f;
                }
            }
        }
        private void FillChartValues()
        {
            if (m_RecordedChartValueValidLength >= m_RecordedChartValues.Length)
            {//已填满，处于循环赋值状态
                if (m_RecordedChartValueValidLength >= m_ChartValues.Length)
                {
                    if (m_RecordedChartValueStartIndex >= m_ChartValues.Length)
                    {//取Source前半截的后半截，拷贝到Dest
                        int length = m_ChartValues.Length;
                        int sourceIndex = m_RecordedChartValueStartIndex - length;
                        int destIndex = 0;
                        System.Array.Copy(m_RecordedChartValues, sourceIndex, m_ChartValues, destIndex, length);
                    }
                    else
                    {
                        int mp = 0;
                        {//取Source前半截，拷贝到Dest后半截
                            int length = m_RecordedChartValueStartIndex;
                            mp = length;
                            int sourceIndex = 0;
                            int destIndex = m_ChartValues.Length - length;
                            System.Array.Copy(m_RecordedChartValues, sourceIndex, m_ChartValues, destIndex, length);
                        }
                        {//取Source后半截的后半截，拷贝到Dest前半截
                            int length = (m_ChartValues.Length - mp);
                            int sourceIndex = m_RecordedChartValueValidLength - length;
                            int destIndex = 0;
                            System.Array.Copy(m_RecordedChartValues, sourceIndex, m_ChartValues, destIndex, length);
                        }
                    }
                }
                else
                {
                    int mp = 0;
                    {//取Source前半截，拷贝到Dest后半截
                        int length = m_RecordedChartValueStartIndex;
                        mp = length;
                        int sourceIndex = 0;
                        int destIndex = m_ChartValues.Length - length;
                        System.Array.Copy(m_RecordedChartValues, sourceIndex, m_ChartValues, destIndex, length);
                    }
                    {//取Source后半截，拷贝到Dest后半截
                        int length = (m_RecordedChartValueValidLength - m_RecordedChartValueStartIndex);
                        int sourceIndex = m_RecordedChartValueStartIndex;
                        int destIndex = m_ChartValues.Length - mp - length;
                        System.Array.Copy(m_RecordedChartValues, sourceIndex, m_ChartValues, destIndex, length);
                    }
                }
            }
            else
            {//未填满
                if (m_RecordedChartValueValidLength >= m_ChartValues.Length)
                {//取Source后半截，拷贝到Dest
                    int length = m_ChartValues.Length;
                    int sourceIndex = m_RecordedChartValueValidLength - length;
                    int destIndex = 0;
                    System.Array.Copy(m_RecordedChartValues, sourceIndex, m_ChartValues, destIndex, length);
                }
                else
                {//取Source，拷贝到Dest后半截
                    int length = m_RecordedChartValueValidLength;
                    int sourceIndex = 0;
                    int destIndex = m_ChartValues.Length - length;
                    System.Array.Copy(m_RecordedChartValues, sourceIndex, m_ChartValues, destIndex, length);
                }
            }
        }
        public abstract void DoUpdateFrame();
        public abstract void DoRecord();
        public void DoRender()
        {
            if (m_RenderTextureChart == null) return;
            if (m_MaterialChart == null) return;

            Profiler.BeginSample("render profiling chart");

            FillChartValues();

            m_MaterialChart.SetFloat("_BackroungTransparent", m_ConfigAppearance.backroungTransparent);
            m_MaterialChart.SetFloat("_ChartTransparent", m_ConfigAppearance.chartTransparent);
            //
            m_MaterialChart.SetColor("_LowColor", s_LowColor);
            m_MaterialChart.SetColor("_MiddlingColor", s_MiddlingColor);
            m_MaterialChart.SetColor("_HighColor", s_HighColor);
            m_MaterialChart.SetColor("_VeryHighColor", s_VeryHighColor);
            //
            m_MaterialChart.SetFloat("_MiddlingValue", m_Config.middlingValue);
            m_MaterialChart.SetFloat("_HighValue", m_Config.highValue);
            m_MaterialChart.SetFloat("_VeryHighValue", m_Config.veryHighValue);
            m_MaterialChart.SetFloat("_FullValue", CalculateFullValue());
            //
            m_MaterialChart.SetFloatArray("_ChartValues", m_ChartValues);
            Graphics.Blit(null, m_RenderTextureChart, m_MaterialChart);

            Profiler.EndSample();
        }

        public abstract void DoGUI(bool foldout);
        ///////////////////////////////////////////////////////////////////////
        protected static string ConvertColor32ToRichtextColor(Color c)
        {
            Color32 c32 = c;
            return $"#{c32.r:x2}{c32.g:x2}{c32.b:x2}{c32.a:x2}";
        }

        protected float CalculateFullValue()
        {
            float curFullValue = Mathf.Max(m_MaxRecordedPeakValue * 1.05f, m_Config.fullValue);
            return curFullValue;
        }
        protected void GetLevelColor(float v, ref Color c)
        {
            if (v >= m_Config.veryHighValue)
            {
                c = s_VeryHighColor;
            }
            else if (v >= m_Config.highValue)
            {
                c = s_HighColor;
            }
            else if (v >= m_Config.middlingValue)
            {
                c = s_MiddlingColor;
            }
            else
            {
                c = s_LowColor;
            }
            c.a = 1.0f;
        }
        protected string GetLevelRichtextColorString(float v)
        {
            string s;
            if (v >= m_Config.veryHighValue)
            {
                s = s_RichtextVeryHighColorString;
            }
            else if (v >= m_Config.highValue)
            {
                s = s_RichtextHighColorString;
            }
            else if (v >= m_Config.middlingValue)
            {
                s = s_RichtextMiddlingColorString;
            }
            else
            {
                s = s_RichtextLowColorString;
            }
            return s;
        }
        public static void CalculateSizes(bool foldout, out int fontSize, out int chartWidth, out int chartHeight)
        {
            var fontSizePercent = ThinRLProfilerHubCfg.Instance.overdrawMonitorConfig.fontSize;
            var chartSizePercent = ThinRLProfilerHubCfg.Instance.overdrawMonitorConfig.chartSize;

            fontSize = (int)(Screen.height * fontSizePercent);
            chartWidth = (int)(Screen.width * chartSizePercent);
            chartHeight = chartWidth / 6;
            if (foldout)
            {
                chartWidth /= 2;
                chartHeight /= 2;
            }
        }
    }
}