Shader "ThinRL/ProfilerHub/Debug/AvatarLightMap"
{
    Properties {
        _LightMapTex ("InnerLine(R) AOMask(G) SpecMask(B)", 2D) = "white" {}
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

            sampler2D _LightMapTex;
            
            // vertex input: position, UV
            struct appdata 
            {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
            };

            struct v2f 
            {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
            };
            
            v2f vert (appdata v) 
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex );
                o.uv = v.texcoord;
                
                return o;
            }
            
            half4 frag( v2f i ) : SV_Target 
            {
                return tex2D(_LightMapTex, i.uv);
            }
            ENDCG
        }
    }
}