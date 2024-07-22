using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ThinRL.ProfilerHub.OverdrawMonitor
{
    public class OverdrawMonitorBuiltinRP : OverdrawMonitor
    {
        private Shader replacementShader;
        private string replacementTag;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            replacementShader = Shader.Find("ThinRL/ProfilerHub/Debug/Overdraw");
            replacementTag = "RenderType";
        }

        void OnPostRender()
        {
            CalculateOverdrawFromRT();
        }

        protected override void RecreateTexture()
        {
            base.RecreateTexture();

            if (overdrawTexture == null)
            {
                overdrawTexture = new RenderTexture(m_RenderTextureSize, m_RenderTextureSize, 24, overdrawRTFormat);
                overdrawTexture.name = "Overdraw_" + m_OverdrawCamera?.name;
                overdrawTexture.hideFlags = HideFlags.HideAndDontSave;
                overdrawTexture.enableRandomWrite = false;
            }
        }
        protected override void SetCameraTarget()
        {
            base.SetCameraTarget();

            m_OverdrawCamera.targetTexture = overdrawTexture;
            // ����targetTexture��aspect�����ΪtargetTexture��aspect������Ҫ��������
            m_OverdrawCamera.aspect = targetCamera.aspect;
            m_OverdrawCamera.SetReplacementShader(replacementShader, replacementTag);
        }

        public override void SetReplacementTag(string tag)
        {
            replacementTag = tag;
        }

        public override void ResetReplacementTag()
        {
            replacementTag = "RenderType";
        }

        public override string GetCurReplacementTag()
        {
            return replacementTag;
        }
    }
}