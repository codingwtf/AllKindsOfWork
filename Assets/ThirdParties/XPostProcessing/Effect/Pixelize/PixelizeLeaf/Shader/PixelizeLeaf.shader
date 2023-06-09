﻿

Shader "Hidden/PostProcessing/Pixelate/PixelizeLeaf"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" { }
    }
    
    HLSLINCLUDE

    #include "../../../../ShaderCore/PostProcessing.hlsl"

    half4 _Params;
    #define _PixelSize _Params.x
    #define _PixelRatio _Params.y
    #define _PixelScaleX _Params.z
    #define _PixelScaleY _Params.w

    float2 TrianglePixelizeUV(float2 uv)
    {
        float2 pixelScale = _PixelSize * float2(_PixelScaleX, _PixelScaleY / _PixelRatio);

        //乘以缩放，向下取整，再除以缩放，得到分段UV
        float2 coord = floor(uv * pixelScale) / pixelScale;

        uv -= coord;
        uv *= pixelScale;

        //进行像素偏移处理
        coord += float2(step(1.0 - uv.y, uv.x) / (pixelScale.x), // Leaf X
        step(uv.x, uv.y) / (pixelScale.y)//Leaf Y
        );

        return coord;
    }

    float4 Frag(VaryingsDefault i): SV_Target
    {

        return GetScreenColor(TrianglePixelizeUV(i.uv));
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }
        
        Cull Off
        ZWrite Off
        ZTest Always

        
        Pass
        {
            HLSLPROGRAM

            #pragma vertex VertDefault
            #pragma fragment Frag

            ENDHLSL

        }
    }
}