using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThinRL.ProfilerHub.RTDebug
{
    [CreateAssetMenu(menuName = "ThinRedLine/ProfilerHub/CreateRTDebugSettingAsset")]
    public class ThinRLRTDebugSettingsAsset : ScriptableObject
    {
        [SerializeField]
        private bool m_EnableTextureDebug = true;
        public bool EnableTextureDebug
        {
            get
            {
                return m_EnableTextureDebug;
            }
            set
            {
                m_EnableTextureDebug = value;
                RTDebugger.outputDebugTexture = m_EnableTextureDebug;
                Debug.Log("fzy out m_EnableTextureDebug" + m_EnableTextureDebug);
            }
        }

        [SerializeField]
        private bool m_FullScreen = false;
        public bool FullScreen
        {
            get
            {
                return m_FullScreen;
            }
            set
            {
                m_FullScreen = value;
                RTDebugger.fullScreen = m_FullScreen;
                Debug.Log("fzy out m_FullScreen" + m_FullScreen);
            }
        }

        public int CurTextureIndex
        {
            get { return RTDebugger.curIndex; }
            set { RTDebugger.curIndex = value; Debug.Log("fzy out CurIndex" + RTDebugger.curIndex); }
        }

        public int TextureCount
        {
            get { return RTDebugger.DataCount; }
        }

        [SerializeField, DefaultValue(0)]
        private float m_ColorRangeMin = 0;
        public float ColorRangeMin { get { return m_ColorRangeMin; } }


        [SerializeField, DefaultValue(1)]
        private float m_ColorRangeMax = 1;
        public float ColorRangeMax { get { return m_ColorRangeMax; } }

        [SerializeField, DefaultValue(false)]
        private bool m_Gamma = false;
        public bool Gamma { get { return m_Gamma; } }


        public void Test()
        {

        }
    }

    public struct RTDebugData
    {
        public Rect drawRect;
        public RenderTargetIdentifier identifier;

        public RTDebugData(Rect drawRect, RenderTargetIdentifier identifier)
        {
            this.drawRect = drawRect;
            this.identifier = identifier;
        }
        public override int GetHashCode()
        {
            return identifier.GetHashCode() + drawRect.GetHashCode();
        }
    }


    public static class RTDebugger
    {
        public static bool outputDebugTexture = true;
        public static bool fullScreen = false;
        public static int curIndex = 0;
        public static HashSet<RTDebugData> debugDataSet;
        public static int DataCount
        {
            get
            {
                if (debugDataSet == null) return 0;
                return debugDataSet.Count;
            }
        }
        public static void Init()
        {
            debugDataSet = new HashSet<RTDebugData>(4);

        }
        public static RTDebugData GetDebugDataWithIndex(int index)
        {
            int c = 0;
            foreach (var item in debugDataSet)
            {
                if (c == index) return item;
                c++;
            }
            return default(RTDebugData);
        }
        public static void DrawDebugTexture(Rect drawRect, RenderTargetIdentifier identifier)
        {
            if (outputDebugTexture == false) return;
            if (debugDataSet == null) { Debug.LogWarning("please initialize 'debugDataList'"); return; }
            var data = new RTDebugData(drawRect, identifier);
            if (debugDataSet.Contains(data) == false)
            {
                debugDataSet.Add(data);
            }
            //Debug.Log("fzy ????zxcvzvzx" + data.drawRect + "   " + data.identifier + "   " + debugDataSet.Count);
            //Debug.Log("fzy debugCount:" + DataCount);
        }

        public static void Dispose()
        {
            debugDataSet.Clear();
            debugDataSet = null;
        }
    }

}
