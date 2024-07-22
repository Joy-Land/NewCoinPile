Shader "ThinRL/ProfilerHub/Debug/Depth"
{
    Properties {
        _MainTex("MainTex", 2D) = "white" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _MainTex;

    struct appdata_t 
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f 
    {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
        float3 param : TEXCOORD1;
    };

    v2f vert (appdata_t v)
    {
        v2f o;
        o.vertex = mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, v.vertex));
        o.uv = v.uv;
        // o.vertex.z is negetive
        // 在viewspace转成线性
        o.param.y = (_ProjectionParams.z - _ProjectionParams.y);
        o.param.x = (_ProjectionParams.z + o.vertex.z) / o.param.y;
        o.vertex = mul(UNITY_MATRIX_P, o.vertex);
        o.param.z = o.vertex.z;
        return o;
    }

    float3 hsv2rgb(float3 c) {
        c = float3(c.x, clamp(c.yz, 0.0, 1.0));
        float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
        float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
        return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
    }

    fixed4 frag (v2f i) : SV_Target
    {
        float4 col;
        // 用hsv.h来显示深度，近处变化快，远处变化小
        col.rgb = hsv2rgb(float3(pow(i.param.x, i.param.y * 0.05), 0.9, 0.9));
        col.a = 1;
        clip(tex2D(_MainTex, i.uv).a - 0.5);
        return col;
    }
    ENDCG

    SubShader 
    {
        Tags {"Queue"="Geometry" "RenderType"="Opaque" }

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
        Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout" }

        Pass 
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}