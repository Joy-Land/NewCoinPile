using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ThinRL.ProfilerHub.RTDebug
{
    public class RTDebugRenderFeature : ScriptableRendererFeature
    {
        public Material rtDebugMaterial;
        FinalRTDebugBlitPass rtDebugPass;
        public override void Create()
        {
            if (rtDebugMaterial == null) return;
            //RTDebugger.Init();
            rtDebugPass = new FinalRTDebugBlitPass(RenderPassEvent.AfterRendering + 5, rtDebugMaterial);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (rtDebugMaterial == null) return;
            rtDebugPass.Setup();
            renderer.EnqueuePass(rtDebugPass);
        }

    }
}

