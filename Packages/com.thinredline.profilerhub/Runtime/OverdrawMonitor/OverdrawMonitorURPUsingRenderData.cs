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
    public class Utility
    {
        public class OverdrawRendererConverter
        {
            private int m_RendererIndexInAsset = -1;
            private int m_LastRendererIndexInAsset = -1;
            public void Prepare()
            {
                m_RendererIndexInAsset = GetOverdrawRendererIndexInAsset();
                m_LastRendererIndexInAsset = -1;
            }
            public bool IsValid()
            {
                return (m_RendererIndexInAsset >= 0) ? true : false;
            }
            public void Convert()
            {
                if (!IsValid()) return;

                UniversalRenderPipelineAsset rpAsset = UniversalRenderPipeline.asset;
                if (rpAsset == null) return;


                {
                    Type rpaType = rpAsset.GetType();
                    FieldInfo fieldInfo = rpaType.GetField("m_DefaultRendererIndex", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fieldInfo != null)
                    {
                        m_LastRendererIndexInAsset = (int)(fieldInfo.GetValue(rpAsset));
                        fieldInfo.SetValue(rpAsset, m_RendererIndexInAsset);
                    }
                }
            }
            public void Restore()
            {
                if (!IsValid()) return;

                UniversalRenderPipelineAsset rpAsset = UniversalRenderPipeline.asset;
                if (rpAsset == null) return;
                {
                    Type rpaType = rpAsset.GetType();
                    FieldInfo fieldInfo = rpaType.GetField("m_DefaultRendererIndex", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fieldInfo != null)
                    {
                        fieldInfo.SetValue(rpAsset, m_LastRendererIndexInAsset);
                    }
                }
            }
        }
        public static bool IsUseUniversalRenderPipeline()
        {
            UniversalRenderPipelineAsset rpAsset = UniversalRenderPipeline.asset;
            return (rpAsset == null) ? false : true;
        }


        public static int GetOverdrawRendererIndexInAsset()
        {
            UniversalRenderPipelineAsset rpAsset = UniversalRenderPipeline.asset;
            if (rpAsset == null) return -1;

            int ret = -1;
            Type rpaType = rpAsset.GetType();
            FieldInfo fieldInfo = rpaType.GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                object o = fieldInfo.GetValue(rpAsset);
                ScriptableRendererData[] vs = o as ScriptableRendererData[];
                if (vs != null)
                {
                    int count = vs.Length;
                    for (int i = 0; i < count; i++)
                    {
                        UniversalRendererData data = vs[i] as UniversalRendererData;
                        if (data == null) continue;
                        foreach (ScriptableRendererFeature x in data.rendererFeatures)
                        {
                            if (x == null) continue;
                            if (null == x as RenderDataForURP.ProfilerHubURPRendererFeature) continue;
                            ret = i;
                        }
                    }
                }
            }

            return ret;
        }
    }

    public class OverdrawMonitorURPUsingRenderData : OverdrawMonitor
    {
        //Base�����Ҫһ�������target����֤����Ⱦ����Ļ��ֻ���������̣�������Ⱦ��������û���κ����塣
        //overdrawTexture�������������
        private RenderTexture overdrawBaseCameraTargetTexture = null;
        private Utility.OverdrawRendererConverter m_OverdrawRendererConverter = null;

        protected override void OnEnableInternal()
        {
            base.OnEnableInternal();

            m_OverdrawRendererConverter = new Utility.OverdrawRendererConverter();
            m_OverdrawRendererConverter.Prepare();

            RenderPipelineManager.beginCameraRendering += RenderPipelineManager_BeforeCameraRendering;
            RenderPipelineManager.endCameraRendering += RenderPipelineManager_EndCameraRendering;
        }
        protected override void OnDisableInternal()
        {
            base.OnDisableInternal();
            
            RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_BeforeCameraRendering;
            RenderPipelineManager.endCameraRendering -= RenderPipelineManager_EndCameraRendering;

            if (overdrawBaseCameraTargetTexture != null)
            {
                overdrawBaseCameraTargetTexture.Release();
                UnityEngine.Object.DestroyImmediate(overdrawBaseCameraTargetTexture);
                overdrawBaseCameraTargetTexture = null;
            }
            m_OverdrawRendererConverter = null;
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

                //if (overdrawBaseCameraTargetTexture != null)
                //{
                //    m_OverdrawCamera.targetTexture = overdrawBaseCameraTargetTexture;
                //    if (!Application.isPlaying)
                //    {
                //        // gameObject����ΪDontSaveInEditorʱ����rtҲҪ����rt��hideFlag ΪDontSaveInEditor����Ȼ�ᱨ Releasing render texture that is set to be RenderTexture.active!
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
            // ����targetTexture��aspect�����ΪtargetTexture��aspect������Ҫ��������
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
                //ocd.clearDepth = tcd.clearDepth;������Ծ�Ȼ��ֻ����
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
            {//����statck
                ocd.cameraStack.Clear();

                if (tcd.renderType == CameraRenderType.Base)
                {
                    foreach (Camera tc in tcd.cameraStack)
                    {
                        OverdrawMonitorURPUsingRenderData om = omm.FindOverdrawMonitor(tc) as OverdrawMonitorURPUsingRenderData;
                        if (om == null) continue;
                        if (om.IsBaseCamera()) continue;//������ܻ����߲�һ�£�UICamera�����stack��������������ߣ����ᱻǿ�����ó�Base�����Բ������ӵ�CameraStack��
                        ocd.cameraStack.Add(om.m_OverdrawCamera);
                    }
                }
            }
        }

        private  bool IsBaseCamera()
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

            m_OverdrawRendererConverter.Convert();
            RenderDataForURP.CopyPass.SetOutputTarget(overdrawTexture);
        }
        private void RenderPipelineManager_EndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera != m_OverdrawCamera) return;
            RenderDataForURP.CopyPass.ClearOutputTarget();
            m_OverdrawRendererConverter.Restore();
            CalculateOverdrawFromRT();
        }
    }
}
#endif