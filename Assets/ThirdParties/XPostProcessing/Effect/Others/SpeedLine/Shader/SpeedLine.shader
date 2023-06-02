

Shader "Hidden/PostProcessing/Other/SpeedLine"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" { }
        _NoiseTex ("NoiseTex", 2D) = "white" { }
    }
    
    HLSLINCLUDE

    #include "../../../../ShaderCore/PostProcessing.hlsl"

    float4 _Color;

    TEXTURE2D(_NoiseTex);SAMPLER(sampler_NoiseTex);

    uniform float4 _Params0;
    uniform float4 _Params1;
    
    #define _Center _Params0
    #define _RotateSpeed _Params1.x
    #define _RayMultiply _Params1.y
    #define _RayPower _Params1.z
    #define _Threshold  _Params1.w

    
    half4 Frag(VaryingsDefault input): SV_Target
    {
        half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
        half2 uv = input.uv - _Center.xy;

        half angle = radians(_RotateSpeed * _Time.y);

        half sinAngle, cosAngle;
        sincos(angle, sinAngle, cosAngle);

        half2x2 rotateMatrix0 = half2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
        half2 normalizedUV0 = normalize(mul(rotateMatrix0, uv));

        half2x2 rotateMatrix1 = half2x2(cosAngle, sinAngle, -sinAngle, cosAngle);
        half2 normalizedUV1 = normalize(mul(rotateMatrix1, uv));
        
        half textureMask = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, normalizedUV0).r * SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, normalizedUV1).r;
        half uvMask = pow(_RayMultiply * length(uv), _RayPower);

        half mask = smoothstep(_Threshold - 0.1, _Threshold + 0.1, textureMask * uvMask);
        baseColor.rgb = lerp(baseColor.rgb,_Color.rgb*mask,mask);
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