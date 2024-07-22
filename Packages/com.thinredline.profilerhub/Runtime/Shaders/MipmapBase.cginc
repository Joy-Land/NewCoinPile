#ifndef __MIPMAP_BASE__
    #define __MIPMAP_BASE__


    #include "UnityCG.cginc"
    sampler2D _MainTex;
    float4 _MainTex_TexelSize;
    float4 _MainTex_ST;
    fixed _Cutoff;
    float _MipmapScale;

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

    // 参考https://community.khronos.org/t/mipmap-level-calculation-using-dfdx-dfdy/67480
    // 有些根据需求的调整
    fixed4 MipmapColor(half2 uv)
    {
        // 乘以_MipmapScale排除rt分辨率对结果的影响
        half2 UVdx = ddx(uv) * _MainTex_TexelSize.zw * _MipmapScale;
        half2 UVdy = ddy(uv) * _MainTex_TexelSize.zw * _MipmapScale;
        // 用min因为想按高精度的选取
        half pixelsBy1Texel_sqr = min(dot(UVdx, UVdx), dot(UVdy, UVdy));
        half mipLevel = 0.5 * log2(pixelsBy1Texel_sqr); // == log2(sqrt(pixelsBy1Texel_sqr));
        // 把最高精度的mip认为是1，一个texcel覆盖4pixel的话是mip 3，16 pixel的是mip 4
        mipLevel += 1;

        half4 tex = tex2D(_MainTex, uv);
        fixed4 col = tex;
        half4 mipNeg1Color = half4(0, 0, 1, 0);
        half4 mip0Color = half4(0, 1, 1, 0);
        half4 mip1Color = half4(0, 1, 0, 0);
        half4 mip2Color = half4(1, 1, 0, 0);
        half4 mip3Color = half4(1, 0, 0, 0);
        half4 mip4Color = half4(0.5, 0, 1, 0);

        // 调大scale，缎带过渡越少
        half scale = 3;
        col = mipNeg1Color;
        col = lerp(col, mip0Color, saturate((mipLevel + 1) * scale));
        col = lerp(col, mip1Color, saturate(mipLevel * scale));
        col = lerp(col, mip2Color, saturate((mipLevel - 1) * scale));
        col = lerp(col, mip3Color, saturate((mipLevel - 2) * scale));
        col = lerp(col, mip4Color, saturate((mipLevel - 3) * scale));  

        col.a = tex.a;
        return col;
    }

    float4 frag(v2f i) : SV_Target
    {
        return MipmapColor(i.uv);
    }
    
    fixed4 AlphaTestFrag(v2f i) : SV_Target
    {
        half4 tex = tex2D(_MainTex, i.uv);
        clip(tex.a - _Cutoff);
        return MipmapColor(i.uv);
    }



#endif