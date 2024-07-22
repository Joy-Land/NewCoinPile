
Shader "ThinRL/ProfilerHub/Debug/Overdraw"
{
	// 为了alphaTest时跟原pass接近，需要用到这些属性
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	}

	CGINCLUDE

	#include "UnityCG.cginc"
	sampler2D _MainTex;
	float4 _MainTex_ST;
	fixed _Cutoff;

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
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		return o;
	}

	fixed4 OverdrawColor()
	{
		return fixed4(0.1, 0.04, 0.02, 0);
	}

	float4 frag(v2f i) : SV_Target
	{
		return OverdrawColor();
	}
	
	fixed4 AlphaTestFrag(v2f i) : SV_Target
	{
		half4 tex = tex2D(_MainTex, i.uv);
		clip(tex.a - _Cutoff);
		return OverdrawColor();
	}

	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		Fog { Mode Off }
		Cull Off
		ZWrite Off
		ZTest LEqual
		Blend One One

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

	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		Fog { Mode Off }
		Cull Back
		ZWrite On
		ZTest LEqual
		Blend One One

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

		// alphaTest的pixel应该都被算入overdraw数值中，即使被clip。
		// 但是没被clip的像素才写入深度，并能遮挡住远处的像素降低overdraw
		// 所以分成2个pass分别写深度和颜色
		// 对于opaque和alphaTest，是先遍历pass再遍历对象的，如下
		// foreach passes
		//     foreach objs
		//         DrawObjWithPass();
		// 透明可以通过设置graphicsSetting里或相机的排序模式来影响，优先有z排序，z相同时会优先遍历pass
		// 因为是先遍历pass再遍历物体所以先写depth再写颜色，这样overdraw的值接近真实
		Pass
		{
			ZWrite On
			ZTest LEqual
			Blend Zero One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment AlphaTestFrag
			ENDCG
		}
		Pass
		{
			ZWrite Off
			ZTest LEqual
			Blend One One
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}