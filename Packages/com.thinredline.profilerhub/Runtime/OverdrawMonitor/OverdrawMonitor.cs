using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

/// <summary> This is a singleton component that is responsible for measuring overdraw information
/// on the main camera. You shouldn't add this compoenent manually, but use the Instance getter to
/// access it.
/// 
/// The measurements process is done in two passes. First a new camera is created that will render
/// the scene into a texture with high precission, the texture is called overdrawTexture. This texture
/// contains the information how many times a pixel has been overdrawn. After this step a compute shader
/// is used to add up all the pixels in the overdrawTexture and stores the information into this component.
/// 
/// We say this tool measures exactly the amount of overdraw, but it does so only in certain cases. In other
/// cases the error margin is very small. This is because of the nature of the compute shaders. Compute
/// shaders should operate in batches in order for them to be efficient. In our case the compute shader 
/// batch that sums up the results of the first render pass has a size of 32x32. This means that if the
/// pixel size of the camera is not divisible by 32, then the edge pixels that don't fit in range won't
/// be processed. But since we usually have huge render targets (in comparison to 32x32 pixel blocks) and
/// the error comes from the part of the image that is not important, this is acceptable. 
/// </summary>
namespace ThinRL.ProfilerHub.OverdrawMonitor
{
    [ExecuteAlways]
    public abstract class OverdrawMonitor : MonoBehaviour
    {
        public static OverdrawMonitor GetOverdrawMonitor(Camera targetCam, CDOverdrawMonitor config)
        {
            string name = targetCam.name + "-OverdrawMonitor";
            var go = GameObject.Find(name);

            OverdrawMonitor om = null;
            if (go == null)
            {
                go = new GameObject(name);
            }
            else
            {
                om = go.GetComponent<OverdrawMonitor>();
                if (om == null || !om.IsTargetCamera(targetCam))
                {
                    go = new GameObject(name);
                }
            }

            om = go.GetComponent<OverdrawMonitor>();
            if (om == null)
            {
                go.SetActive(false);
#if ThinRL_CORE_RENDERPIPLELINE_USING_URP

#if     UNITY_2020_2_OR_NEWER
                om = go.AddComponent<OverdrawMonitorURP>();
#else
                om = go.AddComponent<OverdrawMonitorURPUsingRenderData>();
#endif

#else
                om = go.AddComponent<OverdrawMonitorBuiltinRP>();
#endif
                om.targetCamera = targetCam;
                om.m_Config = config;
                go.SetActive(true);
            }
            return om;
        }

        private CDOverdrawMonitor m_Config = null;

        protected Camera targetCamera;
        protected Camera m_OverdrawCamera;
        public Camera overdrawCamera { get { return m_OverdrawCamera; } }

        [HideInInspector]        
        public RenderTexture overdrawTexture { get; protected set; }
        public const RenderTextureFormat overdrawRTFormat = RenderTextureFormat.ARGBHalf;

        private ComputeShader m_ComputeShader = null;

        // 用方型rt存overdraw值，即使跟实际aspect不一样也不影响平均overdraw值的计算
        // 需要是m_CSGroupThreadSize的倍数，已达到充分分配rt到group
        protected const int m_RenderTextureSize = 256;
        // ios上最多512，用32*32爆掉了，改成16*16==256
        // Metal: Compute shader (OverdrawParallelReduction.CSMain) has 1024 threads in a threadgroup which is more than the supported max of 512 in this device
        const int m_CSGroupThreadSize = 16;   // 一个cs group有16x16个thread，compute shader里有写死的常量
        
        private const int dataSize = (m_RenderTextureSize / m_CSGroupThreadSize) * (m_RenderTextureSize / m_CSGroupThreadSize);
        private float[] m_InputData = new float[dataSize];
        private float[] m_ResultData = new float[dataSize];
        private ComputeBuffer m_ResultBuffer = null;

        private Queue<AsyncGPUReadbackRequest> requests = new Queue<AsyncGPUReadbackRequest>();
        // ========= Results ========
        // Last measurement
        /// <summary> The number of shaded fragments in the last frame. </summary>
        public long TotalShadedFragments { get; private set; }
        /// <summary> The overdraw ration in the last frame. </summary>
        public float OverdrawRatio { get; private set; }

        // Sampled measurement
        /// <summary> Number of shaded fragments in the measured time span. </summary>
        public long IntervalShadedFragments { get; private set; }
        /// <summary> The average number of shaded fragments in the measured time span. </summary>
        public float IntervalAverageShadedFragments { get; private set; }
        /// <summary> The average overdraw in the measured time span. </summary>
        public float IntervalAverageOverdraw { get; private set; }
        public float AccumulatedAverageOverdraw { get { return accumulatedIntervalOverdraw / intervalFrames; } }

        // Extreems
        /// <summary> The maximum overdraw measured. </summary>
        public float MaxOverdraw { get; private set; }

        private long accumulatedIntervalFragments;
        private float accumulatedIntervalOverdraw;
        private long intervalFrames;

        private float intervalTime = 0;
        //public float SampleTime = 1;

        /// <summary> An empty method that can be used to initialize the singleton. </summary>
        public void Touch() { }

#region Measurement magic

#if UNITY_EDITOR
        void OnPlayStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
            {
                requests.Clear();
            }
        }
#endif

        // #if UNITY_EDITOR
        //     public void SubscribeToPlayStateChanged()
        //     {
        //         UnityEditor.EditorApplication.playmodeStateChanged -= OnPlayStateChanged;
        //         UnityEditor.EditorApplication.playmodeStateChanged += OnPlayStateChanged;
        //     }

        //     private void OnPlayStateChanged()
        //     {
        //         if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && UnityEditor.EditorApplication.isPlaying)
        //         {
        //             Dispose();
        //         }
        //     }
        // #endif
        public bool IsDead()
        {
            return targetCamera == null || targetCamera.enabled == false ||
                targetCamera.gameObject.activeInHierarchy == false;
        }

        protected virtual void AwakeInternal()
        {
            // #if UNITY_EDITOR
            //         // Since this emulation always turns on by default if on mobile platform. With the emulation
            //         // turned on the tool won't work.
            //         //UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Graphics Emulation/No Emulation");
            //         SubscribeToPlayStateChanged();
            // #endif

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayStateChanged;
#endif

            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
            //gameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInInspector;
            //if (!Application.isPlaying)
            {
                gameObject.hideFlags = HideFlags.DontSave;
            }

            // Prepare the camera that is going to render the scene with the initial overdraw data.
            m_OverdrawCamera = GetComponent<Camera>();
            if (m_OverdrawCamera == null) m_OverdrawCamera = gameObject.AddComponent<Camera>();
            if (Application.isPlaying)
            {
                m_OverdrawCamera.hideFlags = HideFlags.HideInInspector;
            }
            //RecreateTexture();
            //RecreateComputeBuffer();

            m_ComputeShader = Resources.Load<ComputeShader>("OverdrawParallelReduction");

            for (int i = 0; i < m_InputData.Length; i++) m_InputData[i] = 0;
        }
        protected virtual void OnEnableInternal()
        {
            requests.Clear();
            m_OverdrawCamera.enabled = true;
        }
        protected virtual void OnDisableInternal()
        {
            // OnDestroy();
            m_OverdrawCamera.enabled = false;
            OverdrawRatio = -1.0f;

            if (overdrawTexture != null)
            {
                overdrawTexture.Release();
                UnityEngine.Object.DestroyImmediate(overdrawTexture);
                overdrawTexture = null;
            }
        }
        protected virtual void LateUpdateInternal()
        {
            if (targetCamera == null)
            {
                this.enabled = false;
                return;
            }
            if (!m_OverdrawCamera) return;

            CopyCameraFrom(targetCamera);

            RecreateTexture();

            SetCameraTarget();

            RenderingAsCanvasCamera();

            float sampleTime = (m_Config == null) ? 1.0f : m_Config.sampleTime;
            intervalTime += Time.deltaTime;
            if (intervalTime > sampleTime)
            {
                IntervalShadedFragments = accumulatedIntervalFragments;
                IntervalAverageShadedFragments = (float)accumulatedIntervalFragments / intervalFrames;
                IntervalAverageOverdraw = (float)accumulatedIntervalOverdraw / intervalFrames;

                intervalTime -= sampleTime;

                accumulatedIntervalFragments = 0;
                accumulatedIntervalOverdraw = 0;
                intervalFrames = 0;
            }
        }

        protected virtual void OnDestroyInternal()
        {
            if (m_OverdrawCamera != null)
            {
                m_OverdrawCamera.targetTexture = null;
            }

            m_ComputeShader = null;
            
            if (m_ResultBuffer != null)
            {
                m_ResultBuffer.Release();
                m_ResultBuffer = null;
            }

            if (overdrawTexture != null)
            {
                overdrawTexture.Release();
                UnityEngine.Object.DestroyImmediate(overdrawTexture);
                overdrawTexture = null;
            }
            
        }
        public void Awake()
        {
            AwakeInternal();
        }
        public void OnEnable()
        {
            OnEnableInternal();
        }

        public void OnDisable()
        {
            OnDisableInternal();
        }

        public void LateUpdate()
        {
            LateUpdateInternal();
        }
        public void OnDestroy()
        {
            OnDestroyInternal();
        }

        protected virtual void CopyCameraFrom(Camera target)
        { 
            if (target == null) return;
            if (m_OverdrawCamera == target) return;
           // Debug.Log("fzy ccc:" + target.name + "   " + target.GetHashCode() + "   " + target.clearFlags +"  " + m_OverdrawCamera.name + "   " + m_OverdrawCamera.GetHashCode() + "   " + m_OverdrawCamera.clearFlags);
            m_OverdrawCamera.CopyFrom(target);

            //m_OverdrawCamera.depth = m_CameraBasePrior;
            m_OverdrawCamera.clearFlags = CameraClearFlags.SolidColor;
            m_OverdrawCamera.backgroundColor = Color.black;
            {
                // bug: 选中相机时hdr会被关掉 
                // 同时选中正常相机和overdraw相机就会因为overdraw相机关了hdr，正常相机也关闭了
                // hook到allowHDR的设置也没找到原因，所以先允许overdraw相机用hdr，不影响数值即可
                // m_OverdrawCamera.allowHDR = false;
            }

            m_OverdrawCamera.transform.position = targetCamera.transform.position;
            m_OverdrawCamera.transform.rotation = targetCamera.transform.rotation;
        }
        /// <summary> Checks if the overdraw texture should be updated. This needs to happen if the main camera
        /// configuration changes. </summary>
        protected virtual void RecreateTexture()
        {            

        }

        protected virtual void SetCameraTarget()
        {
        }
        private void RecreateComputeBuffer()
        {
            if (m_ResultBuffer != null) return;
            m_ResultBuffer = new ComputeBuffer(m_ResultData.Length, 4);
        }

        public void Dispose()
        {
            if (this != null)
            {
                GameObject.DestroyImmediate(this.gameObject);
            }

        }
        void RenderingAsCanvasCamera()
        {
            OverdrawMonitorManager omm = OverdrawMonitorManager.Instance;
            Canvas canvas;
            if (!omm.TryGetUIRootCanvas(targetCamera, out canvas)) return;
            if (canvas == null) return;

            RenderMode oldMode = canvas.renderMode;
            canvas.renderMode = RenderMode.WorldSpace;
            m_OverdrawCamera.Render();
            m_OverdrawCamera.enabled = false;
            canvas.renderMode = oldMode;
        }        

        void CheckReqQueue()
        {
            while (requests.Count > 0)
            {
                var req = requests.Peek();

                if (req.hasError)
                {
                    Debug.LogError("overdraw result GPU readback error detected.");
                    requests.Dequeue();
                }
                else if (req.done)
                {
                    UnityEngine.Profiling.Profiler.BeginSample("calculateOverdraw");
                    req.GetData<float>(0).CopyTo(m_ResultData);
                    // m_ResultBuffer.GetData();
                    // Getting the results
                    calculateOverdraw(m_ResultData);
                    requests.Dequeue();
                    UnityEngine.Profiling.Profiler.EndSample();
                }
                else
                {
                    break;
                }
            }
        }

        void calculateOverdraw(float[] rstData)
        {
            double sum = 0;
            for (int i = 0; i < rstData.Length; i++)
            {
                sum += rstData[i];
            }
            OverdrawRatio = (float)(sum / (m_RenderTextureSize * m_RenderTextureSize));

            TotalShadedFragments = (long)sum;
            accumulatedIntervalFragments += TotalShadedFragments;
            accumulatedIntervalOverdraw += OverdrawRatio;
            intervalFrames++;

            if (OverdrawRatio > MaxOverdraw) MaxOverdraw = OverdrawRatio;
        }

        protected void CalculateOverdrawFromRT()
        {
            int maxRequestQueueLength = (m_Config == null) ? 8 : m_Config.maxRequestQueueLength;
            // 请求队列满了后就不计算了
            if (requests.Count < maxRequestQueueLength 
                && overdrawTexture != null 
                && m_ComputeShader != null)
            {
#if UNITY_EDITOR
                if (!EditorApplication.isPaused)
#else
                if (true)
#endif
                {
                    // 在创建本帧计算任务
                    int kernel = m_ComputeShader.FindKernel("CSMain");

                    RecreateComputeBuffer();

                    // ios上最多512，用32*32爆掉了，改成16*16==256
                    // Metal: Compute shader (OverdrawParallelReduction.CSMain) has 1024 threads in a threadgroup which is more than the supported max of 512 in this device
                    int xGroups = (m_RenderTextureSize / m_CSGroupThreadSize);
                    int yGroups = (m_RenderTextureSize / m_CSGroupThreadSize);
                    // Setting up the data
                    m_ResultBuffer.SetData(m_InputData);
                    m_ComputeShader.SetTexture(kernel, "Overdraw", overdrawTexture);
                    m_ComputeShader.SetBuffer(kernel, "Output", m_ResultBuffer);
                    m_ComputeShader.SetInt("GroupX", xGroups);


                    // Summing up the fragments
                    m_ComputeShader.Dispatch(kernel, xGroups, yGroups, 1);
                    if (!Application.isPlaying)
                    {
                        // 在unityEditor下非play模式时，使用同步获取数据模式
                        UnityEngine.Profiling.Profiler.BeginSample("editor getData");
                        m_ResultBuffer.GetData(m_ResultData);
                        calculateOverdraw(m_ResultData);
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                    else
                    {
                        // 在play模式时，使用异步获取数据模式
                        requests.Enqueue(AsyncGPUReadback.Request(m_ResultBuffer));
                    }
                }
            }
            else
            {
                Debug.Log("overdraw 回读队列满, Too many requests.");
            }

            if (Application.isPlaying)
            {
                // 在play模式时， 异步请求队列才会有数据
                CheckReqQueue();
            }
        }

#endregion
#region Measurement control methods
        public void StartMeasurement()
        {
            enabled = true;
            m_OverdrawCamera.enabled = true;
        }

        public void Stop()
        {
            enabled = false;
            m_OverdrawCamera.enabled = false;
        }

        //public void SetSampleTime(float time)
        //{
        //    SampleTime = time;
        //}

        public void ResetSampling()
        {
            accumulatedIntervalOverdraw = 0;
            accumulatedIntervalFragments = 0;
            intervalTime = 0;
            intervalFrames = 0;
        }

        public void ResetExtreemes()
        {
            MaxOverdraw = 0;
        }

        public void Restart()
        {
            Stop();
            StartMeasurement();
            ResetSampling();
            ResetExtreemes();
        }

        public bool IsTargetCamera(Camera cam)
        {
            return targetCamera == cam;
        }

        public bool IsSelfCamera(Camera cam)
        {
            return m_OverdrawCamera == cam;
        }

        public string GetTargetCameraName()
        {
            return (targetCamera != null) ? targetCamera.name : "Unknow";
        }
        public virtual void SetReplacementTag(string tag)
        {
            
        }

        public virtual void ResetReplacementTag()
        {
        }

        public virtual string GetCurReplacementTag()
        {
            return "";
        }

#endregion
    }
}