#if ThinRL_CORE_RENDERPIPLELINE_USING_URP

using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;


namespace ThinRL.ProfilerHub.OverdrawMonitor
{
    internal enum OverdrawProfileId : int
    {
        OverdrawBegin = 0,
        OverdrawEnd = 1,
        OverdrawOpaquePass = 2,
        OverdrawTransparentPass = 3,
    }
    public class OverdrawPass : ScriptableRenderPass
    {
        private string profilerTag;
        private FilteringSettings filteringSettings;
        private List<ShaderTagId> tagIdList = new List<ShaderTagId>();
#if UNITY_2020_2_OR_NEWER
        private ProfilingSampler m_ProfilingSampler;
#endif
        private bool isOpaque;
        private Material material;

        public OverdrawPass(string profilerTag, RenderQueueRange renderQueueRange, Shader shader, bool isOpaque)
        {
            this.profilerTag = profilerTag;
            this.isOpaque = isOpaque; 
#if UNITY_2020_2_OR_NEWER
            m_ProfilingSampler = ProfilingSampler.Get(isOpaque == true ? OverdrawProfileId.OverdrawOpaquePass : OverdrawProfileId.OverdrawTransparentPass);
#endif
            tagIdList.Add(new ShaderTagId("UniversalForward"));
            tagIdList.Add(new ShaderTagId("LightweightForward"));
            tagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
            filteringSettings = new FilteringSettings(renderQueueRange, LayerMask.NameToLayer("Everything"));

            material = CoreUtils.CreateEngineMaterial(shader);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
#if UNITY_2020_2_OR_NEWER
            using (new ProfilingScope(cmd, m_ProfilingSampler))
#endif
            {
                cmd.Clear();
                context.ExecuteCommandBuffer(cmd);
                //var camera = renderingData.cameraData.camera;
                //if (isOpaque)
                //{
                //    if (renderingData.cameraData.isSceneViewCamera ||
                //        (camera.TryGetComponent(out UniversalAdditionalCameraData urpCameraData) &&
                //        urpCameraData.renderType == CameraRenderType.Base))
                //    {
                //        cmd.ClearRenderTarget(true, true, Color.black);
                //    }
                //}
                //context.ExecuteCommandBuffer(cmd);
                //cmd.Clear();

                var sortFlags = isOpaque ? renderingData.cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;
                var drawSettings = CreateDrawingSettings(tagIdList, ref renderingData, sortFlags);
                drawSettings.overrideMaterial = material;
                drawSettings.enableDynamicBatching = renderingData.supportsDynamicBatching;
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }
            CommandBufferPool.Release(cmd);
        }
    }
    public class BeginPass : ScriptableRenderPass
    {
        private string profilerTag;
#if UNITY_2020_2_OR_NEWER
        private readonly ProfilingSampler m_ProfilingSampler;
#endif

        public BeginPass(string profilerTag)
        {
            this.profilerTag = profilerTag;
#if UNITY_2020_2_OR_NEWER
            m_ProfilingSampler = ProfilingSampler.Get(OverdrawProfileId.OverdrawBegin);
#endif
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
#if UNITY_2020_2_OR_NEWER
            using (new ProfilingScope(cmd, m_ProfilingSampler))
#endif
            {
                cmd.Clear();
                context.ExecuteCommandBuffer(cmd);

                Camera camera = renderingData.cameraData.camera;
                bool clearDepth = renderingData.cameraData.clearDepth;
                UniversalAdditionalCameraData urpCameraData;
                if (camera.TryGetComponent(out urpCameraData))
                {
                    RenderTargetIdentifier rtiC;
                    RenderTargetIdentifier rtiD;
#if UNITY_2020_2_OR_NEWER
                    rtiC = renderingData.cameraData.renderer.cameraColorTarget;
                    rtiD = renderingData.cameraData.renderer.cameraDepthTarget;
#else
                    {
                        Type cdType = renderingData.cameraData.GetType();
                        FieldInfo fieldInfo = cdType.GetField("renderer", BindingFlags.NonPublic | BindingFlags.Instance);
                        ScriptableRenderer renderer = fieldInfo.GetValue(renderingData.cameraData) as ScriptableRenderer;
                        if (renderer != null)
                        {
                            rtiC = renderer.cameraColorTarget;
                            rtiD = renderer.cameraDepth;
                        }
                        else
                        {
                            rtiC = BuiltinRenderTextureType.CameraTarget;
                            rtiD = BuiltinRenderTextureType.CameraTarget;
                        }
                    }
#endif

                    cmd.SetRenderTarget(rtiC, rtiD);

                    if (urpCameraData.renderType == CameraRenderType.Base)
                    {
                        cmd.ClearRenderTarget(true, true, Color.black);
                    }
                    else
                    {
                        cmd.ClearRenderTarget(clearDepth, true, Color.black);
                    }
                }
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }
            CommandBufferPool.Release(cmd);
        }
    }
    public class EndPass : ScriptableRenderPass
    {
        private static readonly RenderTargetIdentifier k_TargetNone = new RenderTargetIdentifier(BuiltinRenderTextureType.None);
        private string profilerTag;
#if UNITY_2020_2_OR_NEWER
        private readonly ProfilingSampler m_ProfilingSampler;
#endif
        private RenderTargetIdentifier m_RenderTarget = k_TargetNone;
        private RenderTargetIdentifier m_OutputTarget = k_TargetNone;

        public EndPass(string profilerTag)
        {
            this.profilerTag = profilerTag;
#if UNITY_2020_2_OR_NEWER
            m_ProfilingSampler = ProfilingSampler.Get(OverdrawProfileId.OverdrawEnd);
#endif
        }
        public void setRenderTarget(RenderTargetIdentifier renderTarget)
        {
            m_RenderTarget = renderTarget;
        }
        public void setOutputTarget(RenderTargetIdentifier outputTarget)
        {
            m_OutputTarget = outputTarget;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
#if UNITY_2020_2_OR_NEWER
            using (new ProfilingScope(cmd, m_ProfilingSampler))
#endif
            {
                cmd.Clear();
                context.ExecuteCommandBuffer(cmd);
                Camera camera = renderingData.cameraData.camera;
                UniversalAdditionalCameraData urpCameraData;
                if (camera.TryGetComponent(out urpCameraData))
                {
                    if (!(m_OutputTarget.Equals(k_TargetNone)))
                    {
                        RenderTargetIdentifier rtiC;
                        RenderTargetIdentifier rtiD;
#if UNITY_2020_2_OR_NEWER
                        rtiC = renderingData.cameraData.renderer.cameraColorTarget;
                        rtiD = renderingData.cameraData.renderer.cameraDepthTarget;
#else
                        {
                            Type cdType = renderingData.cameraData.GetType();
                            FieldInfo fieldInfo = cdType.GetField("renderer", BindingFlags.NonPublic | BindingFlags.Instance);
                            ScriptableRenderer renderer = fieldInfo.GetValue(renderingData.cameraData) as ScriptableRenderer;
                            if (renderer != null)
                            {
                                rtiC = renderer.cameraColorTarget;
                                rtiD = renderer.cameraDepth;
                            }
                            else
                            {
                                rtiC = BuiltinRenderTextureType.CameraTarget;
                                rtiD = BuiltinRenderTextureType.CameraTarget;
                            }
                        }
#endif

                        Blit(cmd, rtiC, m_OutputTarget);
                        cmd.SetRenderTarget(rtiC, rtiD);

                        context.ExecuteCommandBuffer(cmd);
                        cmd.Clear();
                    }
                }
            }
            CommandBufferPool.Release(cmd);
        }
    }

    //这是个假货。他并不是真Feature，只表示一个概念，真Feature都是ScriptableRendererFeature的子类。
    public class OverdrawRendererFeature
    {
        private BeginPass beginPass;
        private OverdrawPass opaquePass;
        private OverdrawPass transparentPass;
        private EndPass endPass;

        private Shader opaqueShader = null;
        private Shader transparentShader = null;

        public void Create()
        {
            opaqueShader = Shader.Find("ThinRL/ProfilerHub/Debug/URP/OverdrawOpaque");
            transparentShader = Shader.Find("ThinRL/ProfilerHub/Debug/URP/OverdrawTransparent");
            if (!opaqueShader || !transparentShader)
            {
                return;
            }
            beginPass = new BeginPass("Overdraw Monitor Begin");
            beginPass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
            opaquePass = new OverdrawPass("Overdraw Monitor Opaque", RenderQueueRange.opaque, opaqueShader, true);
            opaquePass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
            transparentPass = new OverdrawPass("Overdraw Monitor Transparent", RenderQueueRange.transparent, transparentShader, false);
            transparentPass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
            endPass = new EndPass("Overdraw Monitor End");
            endPass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
        }
        public void setRenderTarget(RenderTargetIdentifier renderTarget)
        {
            endPass?.setRenderTarget(renderTarget);

        }
        public void setOutputTarget(RenderTargetIdentifier outputTarget)
        {
            endPass?.setOutputTarget(outputTarget);
        }
        public void AddRenderPasses(ScriptableRenderer renderer)
        {
            renderer.EnqueuePass(beginPass);
            renderer.EnqueuePass(opaquePass);
            renderer.EnqueuePass(transparentPass);
            renderer.EnqueuePass(endPass);

        }
    }

    public class OverdrawMonitorURP : OverdrawMonitor
    {
        //Base相机需要一个额外的target来保证不渲染到屏幕，只用于走流程，最终渲染出的像素没有任何意义。
        //overdrawTexture是真正的输出。
        private RenderTexture overdrawBaseCameraTargetTexture;
        private OverdrawRendererFeature m_OverdrawRendererFeature = null;

        protected override void OnEnableInternal()
        {
            base.OnEnableInternal();

            m_OverdrawRendererFeature = new OverdrawRendererFeature();
            m_OverdrawRendererFeature.Create();
            RenderPipelineManager.beginCameraRendering += RenderPipelineManager_BeforeCameraRendering;
            RenderPipelineManager.endCameraRendering += RenderPipelineManager_EndCameraRendering;
        }
        protected override void OnDisableInternal()
        {
            base.OnDisableInternal();
            m_OverdrawRendererFeature = null;
            RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_BeforeCameraRendering;
            RenderPipelineManager.endCameraRendering -= RenderPipelineManager_EndCameraRendering;

            if (overdrawBaseCameraTargetTexture != null)
            {
                overdrawBaseCameraTargetTexture.Release();
                UnityEngine.Object.DestroyImmediate(overdrawBaseCameraTargetTexture);
                overdrawBaseCameraTargetTexture = null;
            }
        }

        protected override void RecreateTexture()
        {
            base.RecreateTexture();

            if (overdrawTexture == null)
            {
                overdrawTexture = new RenderTexture(m_RenderTextureSize, m_RenderTextureSize, 0);
                overdrawTexture.name = "Overdraw_" + m_OverdrawCamera?.name;
                overdrawTexture.hideFlags = HideFlags.HideAndDontSave;
                overdrawTexture.enableRandomWrite = false;
            }

            bool isBase = IsBaseCamera();
            if (isBase)
            {
                if (overdrawBaseCameraTargetTexture == null)
                {
                    overdrawBaseCameraTargetTexture = new RenderTexture(m_RenderTextureSize, m_RenderTextureSize, 24, overdrawRTFormat);
                    overdrawBaseCameraTargetTexture.name = "Overdraw(BaseTarget)" + m_OverdrawCamera?.name;
                    overdrawBaseCameraTargetTexture.hideFlags = HideFlags.HideAndDontSave;
                    overdrawBaseCameraTargetTexture.enableRandomWrite = false;
                }
                RTDebug.RTDebugger.DrawDebugTexture(new Rect(0, 0, 1920 / 10, 1080 / 10), overdrawBaseCameraTargetTexture);
                //if (overdrawBaseCameraTargetTexture != null)
                //{
                //    m_OverdrawCamera.targetTexture = overdrawBaseCameraTargetTexture;
                //    if (!Application.isPlaying)
                //    {
                //        // gameObject设置为DontSaveInEditor时，有rt也要设置rt的hideFlag 为DontSaveInEditor，不然会报 Releasing render texture that is set to be RenderTexture.active!
                //        // overdrawBaseCameraTargetTexture.hideFlags = HideFlags.DontSave;
                //    }
                //}
            }
        }
        protected override void SetCameraTarget()
        {
            base.SetCameraTarget();

            bool isBase = IsBaseCamera();
            if (isBase)
            {
                if (overdrawBaseCameraTargetTexture != null)
                {
                    m_OverdrawCamera.targetTexture = overdrawBaseCameraTargetTexture;
                }
            }
            else
            {
                if (overdrawTexture != null)
                {
                    Rect rc = m_OverdrawCamera.pixelRect;
                    rc.Set(0, 0, overdrawTexture.width, overdrawTexture.height);
                    m_OverdrawCamera.pixelRect = rc;
                }
            }
            // 设置targetTexture后aspect会更新为targetTexture的aspect，所以要重新设置
            m_OverdrawCamera.aspect = targetCamera.aspect;
        }
        protected override void CopyCameraFrom(Camera target)
        {
            base.CopyCameraFrom(target);

            if (target == null) return;
            if (m_OverdrawCamera == target) return;

            UniversalAdditionalCameraData ocd = m_OverdrawCamera.GetUniversalAdditionalCameraData();
            if (ocd == null) return;

            UniversalAdditionalCameraData tcd = target.GetUniversalAdditionalCameraData();
            if (tcd == null) return;

            OverdrawMonitorManager omm = OverdrawMonitorManager.Instance;

            Canvas canvas;
            if (omm.TryGetUIRootCanvas(target, out canvas))
            {
                ocd.renderType = CameraRenderType.Base;
            }
            else
            {
                ocd.renderType = tcd.renderType;
            }

            {
                //ocd.clearDepth = tcd.clearDepth;这个属性居然是只读的
                Type ocdType = ocd.GetType();
                FieldInfo fieldInfo = ocdType.GetField("m_ClearDepth", BindingFlags.NonPublic | BindingFlags.Instance);
                fieldInfo?.SetValue(ocd, tcd.clearDepth);
            }
            ocd.renderPostProcessing = false;
            ocd.dithering = false;
            ocd.requiresColorOption = CameraOverrideOption.Off;
            ocd.requiresDepthOption = CameraOverrideOption.Off;
            ocd.renderShadows = false;

            if (ocd.renderType == CameraRenderType.Base)
            {//更新statck
                ocd.cameraStack.Clear();

                if (tcd.renderType == CameraRenderType.Base)
                {
                    foreach (Camera tc in tcd.cameraStack)
                    {
                        OverdrawMonitorURP om = omm.FindOverdrawMonitor(tc) as OverdrawMonitorURP;
                        if (om == null) continue;
                        if (om.IsBaseCamera()) continue;//这里可能会两边不一致：UICamera相机是stack里的相机，但在这边，他会被强制设置成Base，所以不能添加到CameraStack里
                        ocd.cameraStack.Add(om.m_OverdrawCamera);
                    }
                }
            }
        }

        private bool IsBaseCamera()
        {
            bool ret = false;
            if (m_OverdrawCamera != null)
            {
                UniversalAdditionalCameraData overdrawCameraAdditionalData = m_OverdrawCamera.GetUniversalAdditionalCameraData();
                if (overdrawCameraAdditionalData != null)
                {
                    if (overdrawCameraAdditionalData.renderType == CameraRenderType.Base)
                    {
                        ret = true;
                    }
                }
            }
            return ret;
        }

        private void RenderPipelineManager_BeforeCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera != m_OverdrawCamera) return;

            UniversalAdditionalCameraData overdrawCameraAdditionalData = m_OverdrawCamera.GetUniversalAdditionalCameraData();
            if (overdrawCameraAdditionalData != null)
            {
                {//更新statck
                    if (overdrawCameraAdditionalData.renderType == CameraRenderType.Base)
                    {
                        m_OverdrawRendererFeature.setRenderTarget(overdrawBaseCameraTargetTexture);
                    }
                    m_OverdrawRendererFeature.setOutputTarget(overdrawTexture);
                    m_OverdrawRendererFeature?.AddRenderPasses(overdrawCameraAdditionalData.scriptableRenderer);
                }
            }
        }
        private void RenderPipelineManager_EndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera != m_OverdrawCamera) return;
            CalculateOverdrawFromRT();
        }
    }
}
#endif