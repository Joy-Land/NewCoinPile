Shader "Hidden/Overdraw-Opaque"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" "ShaderModel"="4.5"}
		LOD 300

		Pass
		{
			ZWrite On
			ZTest LEqual
			Blend One One
			Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				float3 vertexWS = TransformObjectToWorld(v.vertex.xyz);
				o.vertex = TransformWorldToHClip(vertexWS.xyz);
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				return half4(0.1, 0.04, 0.02, 1);
			}

			ENDHLSL
		}
	}
}
