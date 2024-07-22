
Shader "ThinRL/ProfilerHub/Debug/MipmapTransparent"
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
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		Fog { Mode Off }
		Cull Off
		ZWrite Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			// 临时解决角色的Always被disable后debug view也不渲染的问题，换成另一个可用的lightMode
			Tags { "LightMode" = "ForwardBase" }
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}

}