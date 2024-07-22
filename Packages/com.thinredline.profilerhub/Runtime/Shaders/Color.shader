Shader "ThinRL/ProfilerHub/Debug/Color"
{
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
    }
    SubShader 
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" }

        Pass 
        {
            ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            struct appdata 
            {
                float4 vertex : POSITION;
                float3 color : COLOR;
                float4 texcoord : TEXCOORD0;
            };

            struct v2f 
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
                float4 uv : TEXCOORD0;
            };
            
            v2f vert (appdata v) 
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex );
                o.color.xyz = v.color.xyz;
                o.color.w = 1.0;
                o.uv = v.texcoord;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target { 
                return i.color; 
                // return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}