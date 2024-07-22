Shader "Hidden/Universal Render Pipeline/RTDebugBlit" {
    SubShader {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass {
            Name "RTDebugBlit"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment Fragment
            #pragma multi_compile_fragment _ _LINEAR_TO_SRGB_CONVERSION
            #pragma multi_compile _ _USE_DRAW_PROCEDURAL
            #pragma multi_compile_fragment _ DEBUG_DISPLAY

            #pragma enable_d3d11_debug_symbols

            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Fullscreen.hlsl"
            //#include "../Shaders/FullScreen.hlsl"
            #include "FullScreen.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/DebuggingFullscreen.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            TEXTURE2D_X(_SourceTex);
            SAMPLER(sampler_SourceTex);

            //(min,max)  50 100
            float _InBlack;
            float _InWhite;
            int _InGamma;

            float GetPixelLevel(float input) {
                float _outBlack = 0;
                float _outWhite = 255;

                float output = input;
                output = max(0, (output - _InBlack));
                output = saturate(pow(output / (_InWhite - _InBlack), 1));
                output = (output * (_outWhite - _outBlack) + _outBlack) / 255;
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.uv;

                half4 col = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_SourceTex, uv);

                if (_InGamma == 1) {
                    col = SRGBToLinear(col);
                }
                //#ifdef _LINEAR_TO_SRGB_CONVERSION
                //col = LinearToSRGB(col);
                //#endif
                //col = SRGBToLinear(col);

                float r = col.r * 255;
                float g = col.g * 255;
                float b = col.b * 255;
                float min_V = _InBlack * 1;
                float max_V = _InWhite * 1;
                float range = max_V - min_V;

                //30   65
                //col.r = max(0,(max_V - min_V)/(255))*(r) + min_V;
                //col.g = max(0,(max_V - min_V)/(255))*(g) + min_V;
                //col.b = max(0,(max_V - min_V)/(255))*(b) + min_V;
                col.r = GetPixelLevel(r);
                col.g = GetPixelLevel(g);
                col.b = GetPixelLevel(b);
                col.rgb = col.rgb ;
                //return col;

                #if defined(DEBUG_DISPLAY)
                    half4 debugColor = 0;

                    if (CanDebugOverrideOutputColor(col, uv, debugColor)) {
                        return debugColor;
                    }
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}