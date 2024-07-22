
Shader "ThinRL/ProfilerHub/Debug/URP/OverdrawTransparent"
{
	// 为了alphaTest时跟原pass接近，需要用到这些属性
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	}

	HLSLINCLUDE

	
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

	TEXTURE2D(_MainTex);           
	SAMPLER(sampler_MainTex);

	CBUFFER_START(UnityPerMaterial)
		half _Cutoff;
		float4 _MainTex_ST;
	CBUFFER_END

	struct appdata
	{
		float4 vertex : POSITION;
		half2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float4 vertex : SV_POSITION;
		half2 uv : TEXCOORD0;
	};

	v2f vert(appdata v)
	{
		v2f o; 
		o.vertex = TransformObjectToHClip(v.vertex.xyz);
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		return o;
	}

	half4 OverdrawColor()
	{
		return half4(0.1, 0.04, 0.02, 0);
	}

	float4 frag(v2f i) : SV_Target
	{
		return OverdrawColor();
	}
	
	half4 AlphaTestFrag(v2f i) : SV_Target
	{		
		half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
		clip(tex.a - _Cutoff);
		return OverdrawColor();
	}

	ENDHLSL

	SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline" }
		Fog { Mode Off }
		Cull Off
		ZWrite Off
		ZTest LEqual
		Blend One One

		Pass
		{
			// 临时解决角色的Always被disable后debug view也不渲染的问题，换成另一个可用的lightMode
			Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
	}
}