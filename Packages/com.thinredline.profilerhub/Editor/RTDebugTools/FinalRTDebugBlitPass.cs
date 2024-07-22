
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace ThinRL.ProfilerHub.RTDebug
{


    public class FinalRTDebugBlitPass : ScriptableRenderPass
    {
        public enum CustomDebug
        {
            FinalBlitDebug
        }
        //只支持四张
        private RenderTargetIdentifier m_Source;
        private Rect m_DrawRect;
        private Material m_FinalDebugBlitMaterial;
        public FinalRTDebugBlitPass(RenderPassEvent evt, Material debugblitMaterial)
        {
            base.profilingSampler = new ProfilingSampler(nameof(FinalRTDebugBlitPass));

            m_FinalDebugBlitMaterial = debugblitMaterial;
            renderPassEvent = evt;
        }

        public void Setup()
        {

        }

        readonly int sourceTexID = Shader.PropertyToID("_SourceTex");
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_FinalDebugBlitMaterial == null)
            {
                Debug.LogErrorFormat("Missing {0}. {1} render pass will not execute. Check for missing reference in the renderer resources.", m_FinalDebugBlitMaterial, GetType().Name);
                return;
            }

            // Note: We need to get the cameraData.targetTexture as this will get the targetTexture of the camera stack.
            // Overlay cameras need to output to the target described in the base camera while doing camera stack.
            ref CameraData cameraData = ref renderingData.cameraData;
            RenderTargetIdentifier cameraTarget = (cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : BuiltinRenderTextureType.CameraTarget;

            bool isSceneViewCamera = cameraData.isSceneViewCamera;
            //sceneview不画debug
            if (isSceneViewCamera || cameraData.camera.clearFlags != CameraClearFlags.Skybox ) return;
            //不在运行不画
            if (Application.isPlaying == false) return;
            //关闭debug不画
            if (RTDebugger.outputDebugTexture == false) return;

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, ProfilingSampler.Get(CustomDebug.FinalBlitDebug)))
            {

                //CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.LinearToSRGBConversion,
                //    cameraData.requireSrgbConversion);
                CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.LinearToSRGBConversion,true);

                for (int i = 0; i < RTDebugger.DataCount; i++)
                {


                    var data = RTDebugger.GetDebugDataWithIndex(i);
                    m_Source = data.identifier;
                    m_DrawRect = data.drawRect;

                    if (RTDebugger.fullScreen)
                    {
                        if (RTDebugger.curIndex != i)
                            continue;
                        m_DrawRect = new Rect(0, 0, cameraData.camera.pixelWidth, cameraData.camera.pixelHeight);
                    }

                    cmd.SetGlobalTexture(sourceTexID, m_Source);


                    CoreUtils.SetRenderTarget(
                        cmd,
                        cameraTarget,
                        RenderBufferLoadAction.Load,
                        RenderBufferStoreAction.Store,
                        ClearFlag.None,
                        Color.black);

                    var drawPos = m_DrawRect.position;
                    Camera camera = cameraData.camera;
                    cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                    cmd.SetViewport(cameraData.camera.pixelRect);

                    var v = new Vector3(
                        ((drawPos.x / cameraData.camera.pixelRect.width) * 2.0f - 1.0f) + (m_DrawRect.width / cameraData.camera.pixelRect.width),
                        ((drawPos.y / cameraData.camera.pixelRect.height) * 2.0f - 1.0f) + (m_DrawRect.height / cameraData.camera.pixelRect.height),
                        0);
                    var m = Matrix4x4.TRS(
                        v,
                        Quaternion.identity,
                        new Vector3((m_DrawRect.width / cameraData.camera.pixelRect.width), (m_DrawRect.height / cameraData.camera.pixelRect.height), 0));

                    cmd.DrawMesh(RenderingUtils.fullscreenMesh, m, m_FinalDebugBlitMaterial);
                    cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
                    cameraData.renderer.ConfigureCameraTarget(cameraTarget, cameraTarget);

                }

            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

}