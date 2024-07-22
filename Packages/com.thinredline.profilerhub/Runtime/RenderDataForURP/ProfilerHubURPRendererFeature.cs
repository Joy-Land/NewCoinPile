#if ThinRL_CORE_RENDERPIPLELINE_USING_URP
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ThinRL.ProfilerHub.RenderDataForURP
{
    public class ClearPass : ScriptableRenderPass
    {
        private string profilerTag;
        //#if !UNITY_2020_2_OR_NEWER
        //        private readonly ProfilingSampler profilingSampler;
        //#endif

        public ClearPass(string profilerTag)
        {
            this.profilerTag = profilerTag;
            //#if !UNITY_2020_2_OR_NEWER
            //            profilingSampler = new ProfilingSampler(profilerTag);
            //#endif
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
            //#if !UNITY_2020_2_OR_NEWER
            //            using (new ProfilingScope(cmd, profilingSampler))
            //#endif
            if (renderingData.cameraData.isSceneViewCamera)
            {
                cmd.ClearRenderTarget(true, true, Color.black);
            }
            else
            {
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
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }

    public class OverdrawPass : ScriptableRenderPass
    {
        private string profilerTag;
        private FilteringSettings filteringSettings;
        private List<ShaderTagId> tagIdList = new List<ShaderTagId>();
#if !UNITY_2020_2_OR_NEWER
        //private readonly ProfilingSampler profilingSampler;
#endif
        private bool isOpaque;
        private Material material;

        public OverdrawPass(string profilerTag, RenderQueueRange renderQueueRange, Shader shader, bool isOpaque)
        {
            this.profilerTag = profilerTag;
            this.isOpaque = isOpaque;

#if !UNITY_2020_2_OR_NEWER
            //profilingSampler = new ProfilingSampler(profilerTag);
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
#if !UNITY_2020_2_OR_NEWER
            //using (new ProfilingScope(cmd, profilingSampler))
#endif
            {
                var sortFlags = isOpaque ? renderingData.cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;
                var drawSettings = CreateDrawingSettings(tagIdList, ref renderingData, sortFlags);
                drawSettings.overrideMaterial = material;
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
    public class CopyPass : ScriptableRenderPass
    {
        private static readonly RenderTargetIdentifier k_TargetNone = new RenderTargetIdentifier(BuiltinRenderTextureType.None);
        private static RenderTargetIdentifier m_OutputTarget = k_TargetNone;
        public static void SetOutputTarget(RenderTargetIdentifier outputTarget)
        {
            m_OutputTarget = outputTarget;
        }
        public static void ClearOutputTarget()
        {
            m_OutputTarget = k_TargetNone;
        }
        private string profilerTag;
#if !UNITY_2020_2_OR_NEWER
        //private readonly ProfilingSampler profilingSampler;
#endif
        public CopyPass(string profilerTag)
        {
            this.profilerTag = profilerTag;
#if !UNITY_2020_2_OR_NEWER
            //profilingSampler = new ProfilingSampler(profilerTag);
#endif
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
#if !UNITY_2020_2_OR_NEWER
            //using (new ProfilingScope(cmd, profilingSampler))
#endif
            {
                if (!renderingData.cameraData.isSceneViewCamera)
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
    public class ProfilerHubURPRendererFeature : ScriptableRendererFeature
	{
#if UNITY_EDITOR
		private ClearPass	 clearPass;
		private OverdrawPass opaquePass;
		private OverdrawPass transparentPass;
		private CopyPass	 copyPass;

		[SerializeField] private Shader opaqueShader = null;
		[SerializeField] private Shader transparentShader = null;
#endif

		public override void Create()
		{
#if UNITY_EDITOR
			if (!opaqueShader || !transparentShader)
			{
				return;
			}

			clearPass = new ClearPass("Overdraw Monitor Clear");
			clearPass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
			opaquePass = new OverdrawPass("Overdraw Monitor Opaque", RenderQueueRange.opaque, opaqueShader, true);
			opaquePass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
			transparentPass = new OverdrawPass("Overdraw Monitor Transparent", RenderQueueRange.transparent, transparentShader, false);
			transparentPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
			copyPass = new CopyPass("Overdraw Monitor Copy");
			copyPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
#endif
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
#if UNITY_EDITOR
			renderer.EnqueuePass(clearPass);
			renderer.EnqueuePass(opaquePass);
			renderer.EnqueuePass(transparentPass);
			renderer.EnqueuePass(copyPass);
#endif
		}
	}
}
#endif