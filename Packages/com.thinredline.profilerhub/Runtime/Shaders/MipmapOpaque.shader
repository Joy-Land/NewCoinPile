
Shader "ThinRL/ProfilerHub/Debug/MipmapOpaque"
{
	Properties {
		// 为了alphaTest时跟原pass接近，需要用到这些属性
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
		
		// 为了跟特效的alpha效果一致
		_TintColor ("Tint Color", Color) = (1, 1, 1, 1)
	}

	CGINCLUDE

	#include "MipmapBase.cginc"
	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		Fog { Mode Off }
		Cull Back
		ZWrite On
		ZTest LEqual
		Blend One Zero

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
	
	SubShader
	{
		Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" }
		Fog { Mode Off }
		Cull Off

		Pass
		{
			ZWrite On
			ZTest LEqual
			Blend One Zero

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment AlphaTestFrag
			ENDCG
		}
	}

	
	// SubShader
	// {
	// 	Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
	// 	Fog { Mode Off }
	// 	Cull Off
	// 	ZWrite Off
	// 	ZTest LEqual
	// 	Blend SrcAlpha OneMinusSrcAlpha

	// 	Pass
	// 	{
	// 		CGPROGRAM
	// 		#pragma vertex vert
	// 		#pragma fragment frag
	// 		ENDCG
	// 	}
	// }
}