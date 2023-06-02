

Shader "Hidden/PostProcessing/Other/GradientRamp"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" { }
        _Color("Main Color", Color) = (1,1,1,1)
    }
    
    HLSLINCLUDE

    #include "../../../../ShaderCore/PostProcessing.hlsl"

    float4 _Color;

    TEXTURE2D(_NoiseTex);SAMPLER(sampler_NoiseTex);

    uniform float4 _Params0;
    #define _Threshold _Params0.x
    #define _UVThresholdMin _Params0.y
    #define _UVThresholdMax _Params0.z

    half4 Frag(VaryingsDefault input): SV_Target
    {
        half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
        #ifndef UNITY_UV_STARTS_AT_TOP
            input.uv.y = 1 - input.uv.y;
        #endif
        float smooth = smoothstep(_UVThresholdMin,_UVThresholdMax,input.uv.y);
        baseColor.rgb = lerp(baseColor.rgb,_Color.rgb,smooth * _Threshold);
        return baseColor;
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