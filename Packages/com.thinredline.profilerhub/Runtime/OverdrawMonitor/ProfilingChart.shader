Shader "ThinRL/ProfilerHub/Debug/ProfilingChart"
{
	Properties
	{
		_BackroungTransparent("BackroungTransparent", Range(0, 1)) = 0.5
		_ChartTransparent("ChartTransparent", Range(0, 1)) = 1.0

		_LowColor("LowColor", Color) = (0, 1, 0, 1)
		_MiddlingColor("MiddingColor", Color) = (1, 1, 0, 1)
		_HighColor("HighColor", Color) = (1, 0.5, 0, 1)
		_VeryHighColor("VeryHighColor", Color) = (1, 0, 0, 1)
		
		_MiddlingValue("MiddlingValue", Float) = 1.0
		_HighValue("HighValue", Float) = 2.0
		_VeryHighValue("VeryHighValue", Float) = 4.0
		_FullValue("FullValue", Float) = 8.0
	}

	SubShader
	{
		Pass
		{
			Cull Off
			ZWrite Off
			ZTest Off
			Blend One Zero

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			fixed _BackroungTransparent;
			fixed _ChartTransparent;

			fixed4 _LowColor;
			fixed4 _MiddlingColor;
			fixed4 _HighColor;
			fixed4 _VeryHighColor;

			float _MiddlingValue;
			float _HighValue;
			float _VeryHighValue;
			float _FullValue;

			//#define MAX_CHART_VALUES_LENGTH (512) 
			#define MAX_CHART_VALUES_LENGTH (128) 
			float _ChartValues[MAX_CHART_VALUES_LENGTH];

			struct si
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};
						
			v2f vert(si i)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.texcoord = i.texcoord;
				return o;
			}

			float calculateChartValue(float u)
			{				
				uint index = (uint)(saturate(u) * (MAX_CHART_VALUES_LENGTH - 1) + 0.5);
				return _ChartValues[index];
			}
			fixed4 calculateColor(float chartValue, float v)
			{
				fixed4 c;
				if (v * _FullValue <= chartValue)
				{
					if (chartValue >= _VeryHighValue)
					{
						c.rgb = _VeryHighColor.rgb;
					}
					else if (chartValue >= _HighValue)
					{
						c.rgb = _HighColor.rgb;
					}
					else if (chartValue >= _MiddlingValue)
					{
						c.rgb = _MiddlingColor.rgb;
					}
					else
					{
						c.rgb = _LowColor.rgb;
					}
					c.a = _ChartTransparent;
				}
				else
				{
					c = fixed4(0, 0, 0, _BackroungTransparent);
				}
				return c;
			}
			fixed4 frag(v2f i) : SV_Target
			{
				half2 uv = i.texcoord;
				//
				float chartValue = calculateChartValue(uv.x);
				//
				return  calculateColor(chartValue, uv.y);
			}

			ENDCG
		}
	}

	//Fallback "Hidden/InternalErrorShader"	
}