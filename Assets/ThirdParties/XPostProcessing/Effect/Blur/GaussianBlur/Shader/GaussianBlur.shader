﻿

Shader "Hidden/PostProcessing/Blur/GaussianBlur"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" { }
    }
    
    HLSLINCLUDE

    #include "../../../../ShaderCore/PostProcessing.hlsl"

    half4 _BlurOffset;
    
    struct v2f
    {
        float4 pos: POSITION;
        float2 uv: TEXCOORD0;
        float4 uv01: TEXCOORD1;
        float4 uv23: TEXCOORD2;
        float4 uv45: TEXCOORD3;
    };
    
    v2f VertGaussianBlur(AttributesDefault v)
    {
        v2f o;
        o.pos = TransformObjectToHClip(v.positionOS.xyz);// float4(v.vertex.xy, 0, 1);
        o.uv = v.uv;//TransformTriangleVertexToUV(o.pos.xy);
        // o.uv = TransformStereoScreenSpaceTex(o.uv, 1.0);
        
        o.uv01 = o.uv.xyxy + _BlurOffset.xyxy * float4(1, 1, -1, -1);
        o.uv23 = o.uv.xyxy + _BlurOffset.xyxy * float4(1, 1, -1, -1) * 2.0;
        o.uv45 = o.uv.xyxy + _BlurOffset.xyxy * float4(1, 1, -1, -1) * 6.0;
        
        return o;
    }
    
    float4 FragGaussianBlur(v2f i): SV_Target
    {
        half4 color = float4(0, 0, 0, 0);
        
        color += 0.40 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
        color += 0.15 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv01.xy);
        color += 0.15 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv01.zw);
        color += 0.10 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv23.xy);
        color += 0.10 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv23.zw);
        color += 0.05 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv45.xy);
        color += 0.05 * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv45.zw);
        
        return color;
    }

    
    float4 FragCombine(VaryingsDefault i): SV_Target
    {
        return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
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

            #pragma vertex VertGaussianBlur
            #pragma fragment FragGaussianBlur
            
            ENDHLSL

        }
        
        Pass
        {
            HLSLPROGRAM

            #pragma vertex VertDefault
            #pragma fragment FragCombine
            
            ENDHLSL

        }
    }
}