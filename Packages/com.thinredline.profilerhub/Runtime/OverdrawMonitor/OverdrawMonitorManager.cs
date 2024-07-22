using System.Text;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Profiling;
using UnityEditor;
using System;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace ThinRL.ProfilerHub.OverdrawMonitor
{
    // 显示游戏中所有相机的overdraw数值
    [ExecuteAlways]
    public class OverdrawMonitorManager : MonoBehaviour
    {
        private ConfigDefine m_Config;
        private List<OverdrawMonitor> m_Monitors;
        private ParticleMonitor m_ParticleMonitor;
        private StringBuilder m_TextBuilder;
        private List<ProfilingChartRendererOverdraw> m_ChartRendererOverdraws;
        private ProfilingChartRendererOverdrawSummary m_ChartRendererOverdrawSummary;
        private ProfilingChartRendererParticle m_ChartRendererParticle;
        private bool m_ChartFoldout = true;

        List<Canvas> m_RootCanvases;


        void Start()
        {
            m_Config = ThinRLProfilerHubCfg.Instance.overdrawMonitorConfig;
            m_TextBuilder = new StringBuilder(512);
            m_Monitors = new List<OverdrawMonitor>();
            m_RootCanvases = new List<Canvas>();

            m_ChartRendererOverdraws = new List<ProfilingChartRendererOverdraw>();
        }

        private void OnDestroy()
        {
            m_RootCanvases?.Clear();
            if (m_ChartRendererOverdraws != null)
            {
                foreach (ProfilingChartRenderer x in m_ChartRendererOverdraws)
                {
                    x.UninitializeRenderer();
                }
                m_ChartRendererOverdraws.Clear();
            }
            if (m_ChartRendererOverdrawSummary != null)
            {
                m_ChartRendererOverdrawSummary.UninitializeRenderer();
                m_ChartRendererOverdrawSummary = null;
            }
            if (m_ChartRendererParticle != null)
            {
                m_ChartRendererParticle.UninitializeRenderer();
                m_ChartRendererParticle = null;
            }
            /////
            if (m_Monitors != null)
            {
                foreach (OverdrawMonitor monitor in m_Monitors)
                {
                    monitor.Dispose();
                }
                m_Monitors.Clear();
            }

            if (m_ParticleMonitor != null)
            {
                m_ParticleMonitor.Dispose();
                m_ParticleMonitor = null;
            }
        }

        public void Update()
        {
            if (m_ChartRendererOverdraws == null)
            {
                return;
            }

            UpdateUIRootCanvas();

            for (int i = m_Monitors.Count - 1; i >= 0; i--)
            {
                if (m_Monitors[i] == null)
                {
                    m_Monitors.RemoveAt(i);
                }
                else if (m_Monitors[i].IsDead())
                {
                    string chartRendererName = m_Monitors[i].name;
                    m_Monitors[i].Dispose();
                    m_Monitors.RemoveAt(i);

                    ReleaseAndRemoveProfilingChartFromList(chartRendererName, m_ChartRendererOverdraws);
                }
            }

            foreach (Camera cam in Camera.allCameras)
            {
                bool isNewTarget = true;
                foreach (OverdrawMonitor monitor in m_Monitors)
                {
                    if (monitor.IsTargetCamera(cam) || monitor.IsSelfCamera(cam))
                    {
                        isNewTarget = false;
                        break;
                    }
                }
                if (isNewTarget)
                {
                    var cameraMonitor = OverdrawMonitor.GetOverdrawMonitor(cam, m_Config.overdrawMonitor);
                    cameraMonitor.transform.SetParent(this.transform, true);
                    m_Monitors.Add(cameraMonitor);

                    {
                        string chartRendererName = cameraMonitor.name;
                        CDOverdrawChart chartConfig = ProfilingChartRendererOverdraw.FetchConfig(m_Config.overdrawChartDefault, m_Config.overdrawChartCustom, cameraMonitor.GetTargetCameraName());
                        ProfilingChartRendererOverdraw chartRenderer = new ProfilingChartRendererOverdraw(chartRendererName, chartConfig, m_Config.chartAppearance, cameraMonitor);
                        chartRenderer.InitializeRenderer();
                        m_ChartRendererOverdraws.Add(chartRenderer);
                    }
                }
            }

            if (m_ChartRendererOverdrawSummary == null)
            {
                string chartRendererName = "OverdrawSummary";
                m_ChartRendererOverdrawSummary = new ProfilingChartRendererOverdrawSummary(chartRendererName, m_Config.overdrawSummaryChart, m_Config.chartAppearance, m_Monitors);
                m_ChartRendererOverdrawSummary.InitializeRenderer();
            }

            if (m_ParticleMonitor == null)
            {
                m_ParticleMonitor = ParticleMonitor.GetMonitor(this.transform, m_Config.particleMonitor);

                {
                    string chartRendererName = m_ParticleMonitor.name;
                    m_ChartRendererParticle = new ProfilingChartRendererParticle(name, m_Config.particleChart, m_Config.chartAppearance, m_ParticleMonitor);
                    m_ChartRendererParticle.InitializeRenderer();
                }
            }
        }

        void LateUpdate()
        {            
            UpdateProfilingChartFrame();
        }

        // screen space - camera模式的canvas只能被那一个ui相机渲染，但是world模式的都可以
        // 所以为了能默认显示，就把模式改成world，按overdraw渲染，再改回默认模式
        void UpdateUIRootCanvas()
        {
            var canvasesPath = m_Config.rootUICanvasPath;
            if (canvasesPath != null && canvasesPath.Count != m_RootCanvases.Count)
            {
                m_RootCanvases.Clear();
                foreach (var v in canvasesPath)
                    m_RootCanvases.Add(GameObject.Find(v)?.GetComponent<Canvas>());
            }
            for (int i = 0; i < m_RootCanvases.Count; ++i)
            {
                var canvas = m_RootCanvases[i];
                if (canvas == null)
                {
                    canvas = GameObject.Find(canvasesPath[i])?.GetComponent<Canvas>();
                    m_RootCanvases[i] = canvas;
                }
            }
        }
        public bool TryGetUIRootCanvas(Camera camera, out Canvas canvas)
        {
            canvas = null;
            if (m_RootCanvases == null) return false;

            bool ret = false;
            for (int i = 0; i < m_RootCanvases.Count; ++i)
            {
                Canvas c = m_RootCanvases[i];
                if (c == null || c.renderMode == RenderMode.WorldSpace) continue;
                if (c.worldCamera != camera) continue;
                canvas = c;
                ret = true;
            }
            return ret;
        }
        bool ReleaseAndRemoveProfilingChartFromList<T>(string name, List<T> chartRenderers) where T : ProfilingChartRenderer
        {
            bool ret = false;
            int count = chartRenderers.Count;
            for (int i = 0; i < count; i++)
            {
                ProfilingChartRenderer chartRenderer = chartRenderers[i];
                if (!chartRenderer.isSelfName(name)) continue;
                chartRenderers.RemoveAt(i);

                chartRenderer.UninitializeRenderer();
                ret = true;
                break;
            }
            return ret;
        }
        float m_RecordIntervalTimeAcc = 0.0f;
        void UpdateProfilingChartFrame()
        {
            float recordInterval = m_Config.chartUpdateInterval;
            bool doOnce = false;
            m_RecordIntervalTimeAcc += Time.deltaTime;
            if (m_RecordIntervalTimeAcc > recordInterval)
            {
                m_RecordIntervalTimeAcc -= recordInterval;
                doOnce = true;
            }

            if (m_Config.forceUpdateEveryFrame)
                doOnce = true;

            if (m_ChartRendererOverdraws != null)
            {
                foreach (ProfilingChartRendererOverdraw x in m_ChartRendererOverdraws)
                {
                    x.DoUpdateFrame();
                    if (doOnce)
                    {
                        x.DoRecord();
                        x.DoRender();
                    }
                }
            }

            if (m_ChartRendererOverdrawSummary != null)
            {
                m_ChartRendererOverdrawSummary.DoUpdateFrame();
                if (doOnce)
                {
                    m_ChartRendererOverdrawSummary.DoRecord();
                    m_ChartRendererOverdrawSummary.DoRender();
                }
            }

            if (m_ChartRendererParticle != null)
            {
                m_ChartRendererParticle.DoUpdateFrame();
                if (doOnce)
                {
                    m_ChartRendererParticle.DoRecord();
                    m_ChartRendererParticle.DoRender();
                }
            }
        }
        public void SetAllOverdrawAsTransparent()
        {
            foreach (OverdrawMonitor monitor in m_Monitors)
            {
                if (!monitor.IsDead())
                {
                    monitor.SetReplacementTag(null);
                }
            }
        }


        public void SetAllOverdrawAsDefault()
        {
            foreach (OverdrawMonitor monitor in m_Monitors)
            {
                if (!monitor.IsDead())
                {
                    monitor.ResetReplacementTag();
                }
            }
        }

        public bool IsOverdrawUsingTransparentTag()
        {
            foreach (OverdrawMonitor monitor in m_Monitors)
            {
                if (!monitor.IsDead())
                {
                    return null == monitor.GetCurReplacementTag();
                }
            }
            return false;
        }

        public string GetShowText()
        {
            if (m_TextBuilder == null)
                m_TextBuilder = new StringBuilder(512);

            m_TextBuilder.Clear();

            {//Particle就一行，比较稳定，放到前面
                if (m_ParticleMonitor != null)
                {
                    m_TextBuilder.AppendLine($"P-Particle total: { m_ParticleMonitor.particleCount }");
                }
            }

            {
                foreach (OverdrawMonitor monitor in m_Monitors)
                {
                    if (!monitor.IsDead())
                    {
                        m_TextBuilder.AppendLine($"O-{monitor.GetTargetCameraName()}: {monitor.OverdrawRatio:F1}");
                    }
                }
            }

            return m_TextBuilder.ToString();
        }

        public float GetAllCameraTotalOverdraw()
        {
            float sum = 0;
            foreach (var m in m_Monitors)
                sum += m.OverdrawRatio;
            return sum;
        }

        public OverdrawMonitor FindOverdrawMonitor(Camera tergetCamera)
        {
            if (tergetCamera == null) return null;

            OverdrawMonitor ret = null;
            foreach (OverdrawMonitor x in m_Monitors)
            {
                if (!x.IsTargetCamera(tergetCamera)) continue;
                ret = x;
                break;
            }
            return ret;
        }

#if UNITY_EDITOR
        void OnGUI()
        {
            Profiler.BeginSample("OverdrawMonitorManager on gui");

            GUILayout.BeginVertical();
            {
                GUILayout.BeginVertical("粒子", "box");
                GUILayout.Space(12.0f);
                if (m_ChartRendererParticle != null)
                {
                    m_ChartRendererParticle.DoGUI(m_ChartFoldout);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Overdraw", "box");
                GUILayout.Space(12.0f);
                if (m_ChartRendererOverdrawSummary != null)
                {
                    m_ChartRendererOverdrawSummary.DoGUI(m_ChartFoldout);
                }
                if (m_ChartRendererOverdraws != null)
                {
                    foreach (ProfilingChartRenderer x in m_ChartRendererOverdraws)
                    {
                        x.DoGUI(m_ChartFoldout);
                    }
                }
                GUILayout.EndVertical();

                if (GUILayout.Button(m_ChartFoldout ? "+" : "-"))
                {
                    m_ChartFoldout = !m_ChartFoldout;
                }
            }
            GUILayout.EndVertical();

            Profiler.EndSample();
        }
#endif

        private static OverdrawMonitorManager s_Instance;
        // 关闭instance访问，直接访问会不走可用性判断
        public static OverdrawMonitorManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    GameObject go = GameObject.Find("OverdrawMonitorManagerGo");
                    if (go == null)
                    {
                        go = new GameObject("OverdrawMonitorManagerGo");
                        if (Application.isPlaying)
                        {
                            DontDestroyOnLoad(go);
                        }
                        if (!Application.isPlaying)
                        {
                            // 标记为DontSaveInEditor需要手动删除gameObject，
                            // 注意切换scene时，gameObject会从hierarchy消失，但依然会每帧调OnGUI，
                            // 从非play切换到play时，还是会调onGUi，但Awake，destroy都没调过，非play切换play之前用C#保存的引用删掉gameObject可以回避这个问题
                            // 并且所有的子节点也要设置为DontSaveInEditor，不然会报 Can't destroy Transform component of 'xxx'. 
                            // 有rt也要设置rt的hideFlag 为DontSaveInEditor，不然会报 Releasing render texture that is set to be RenderTexture.active!
                            go.hideFlags = HideFlags.DontSave;
                        }
                    }

                    OverdrawMonitorManager omm = go.GetComponent<OverdrawMonitorManager>();
                    if (omm == null)
                    {
                        omm = go.AddComponent<OverdrawMonitorManager>();
                    }

                    s_Instance = omm;

                    //s_Instance = GameObject.FindObjectOfType<OverdrawMonitorManager>();
                    //if (s_Instance == null)
                    //{
                    //    GameObject go = new GameObject("OverdrawMonitorManagerGo");
                    //    s_Instance = go.AddComponent<OverdrawMonitorManager>();
                    //    if (Application.isPlaying)
                    //    {
                    //        DontDestroyOnLoad(go);
                    //    }
                    //    if (!Application.isPlaying)
                    //    {
                    //        // 标记为DontSaveInEditor需要手动删除gameObject，
                    //        // 注意切换scene时，gameObject会从hierarchy消失，但依然会每帧调OnGUI，
                    //        // 从非play切换到play时，还是会调onGUi，但Awake，destroy都没调过，非play切换play之前用C#保存的引用删掉gameObject可以回避这个问题
                    //        // 并且所有的子节点也要设置为DontSaveInEditor，不然会报 Can't destroy Transform component of 'xxx'. 
                    //        // 有rt也要设置rt的hideFlag 为DontSaveInEditor，不然会报 Releasing render texture that is set to be RenderTexture.active!
                    //        s_Instance.gameObject.hideFlags = HideFlags.DontSave;
                    //    }
                    //}
                }

                return s_Instance;
            }
        }

        public static bool IsShowing()
        {
            return s_Instance != null && s_Instance.isActiveAndEnabled;
        }

        public static bool SetShowEnabled()
        {
            if (SystemInfo.SupportsRenderTextureFormat(OverdrawMonitor.overdrawRTFormat) == false
                || SystemInfo.supportsComputeShaders == false || SystemInfo.supportsAsyncGPUReadback == false
                )
            {
                return false;
            }
            Instance.gameObject.SetActive(true);
            return true;
        }

        public static void SetShowDisabled()
        {
            if (s_Instance == null)
            {
                return;
            }
            Instance.gameObject.SetActive(false);
        }

        public static float GetAllCameraTotalOverdrawInst()
        {
            if (IsShowing())
            {
                return s_Instance.GetAllCameraTotalOverdraw();
            }
            Debug.LogWarning("需要先调用SetShowEnabled");
            return -1;
        }

        public static void DestroyInstance()
        {
            foreach (var com in Resources.FindObjectsOfTypeAll(typeof(OverdrawMonitorManager)) as OverdrawMonitorManager[])
            {
#if UNITY_EDITOR
                bool isHierarchyGo = !EditorUtility.IsPersistent(com);
#else
                bool isHierarchyGo = true;
#endif
                if (isHierarchyGo)
                {
                    GameObject.DestroyImmediate(com.gameObject);
                }
            }
            s_Instance = null;
        }

        public static bool IsOverdrawUsingTransparent()
        {
            return Instance.IsOverdrawUsingTransparentTag();
        }

        public static void SetOverdrawUsingTransparent(bool isUse)
        {
            if (isUse)
            {
                Instance.SetAllOverdrawAsTransparent();
            }
            else
            {
                Instance.SetAllOverdrawAsDefault();
            }
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void OnInitialize()
        {
            //if (!InitializeOnLoadSetting.GetOrCreateSettings().enableOverdrawMonitorManager)
            //    return;

            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.playModeStateChanged -= OnPlayStateChanged;
                UnityEditor.EditorApplication.playModeStateChanged += OnPlayStateChanged;
                // DestroyInstance();
                // s_Instance = null;
                EditorSceneManager.activeSceneChangedInEditMode -= ChangedActiveScene;
                EditorSceneManager.activeSceneChangedInEditMode += ChangedActiveScene;
            }
        }


        static void OnPlayStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
            {
                // editor模式从play转到非play后重置gameObject和引用
                DestroyInstance();
                ShowOverdrawText();
            }
            else if (playModeStateChange == PlayModeStateChange.ExitingEditMode)
            {
                // editor模式即将进入play时删掉gameObject和用例，因为gameOBject的hideflag为dontsave需要手动删除gameObject
                DestroyInstance();
            }
        }

        static void ChangedActiveScene(Scene current, Scene next)
        {
            // Debug.Log("OverdrawMonitorManager OnSceneLoaded");
            // editor模式打开另一个scene时删除gameObject，清除s_instance的引用不然gameObject会变成幽灵gameObject
            // 在新的scene中新建gameObject
            DestroyInstance();
            ShowOverdrawText();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnAfterSceneLoadRuntimeMethod()
        {
#if UNITY_EDITOR
            //if (!InitializeOnLoadSetting.GetOrCreateSettings().enableRuntimeOverdrawMonitorManager)
            //    return;
#endif

            DestroyInstance();
            ShowOverdrawText();
        }

        static void ShowOverdrawText()
        {
            if(OverdrawMonitorController.GetEnableState())
            {
                if (Application.isEditor)
                    SetShowEnabled();
            }
            else
            {
                SetShowDisabled();
            }
        }

        public static string SupportedRTFormat
        {
            get
            {
                var sb = new StringBuilder();
                foreach (int value in Enum.GetValues(typeof(RenderTextureFormat)))
                {
                    var name = Enum.GetName(typeof(RenderTextureFormat), value);
                    var supported = SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat)value);
                    sb.AppendFormat("{0}:{1} \n", name, supported);
                }
                return sb.ToString();
            }
        }
    }
}
