

Shader "Hidden/PostProcessing/Other/BlackWhite"
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

    half3 luminance(half3 color)
    {
        return dot(color, half3(0.222, 0.707, 0.071));
    }

    //径向模糊
    uniform float4 _Params0;
    uniform float4 _Params1;
    uniform float4 _Params2;
    
    #define _BlurRadius _Params0.x
    #define _Iteration _Params0.y
    #define _RadialCenter _Params0.zw
    
    #define GreyThreshold _Params1.x;
    #define Luminance _Params1.y;
    #define Contrast _Params1.z;
    #define NoiseTexValue _Params2
    
    

    half4 RadialBlur(VaryingsDefault i)
    {
        float2 blurVector = (_RadialCenter - i.uv.xy) * _BlurRadius;
            
        half4 acumulateColor = half4(0, 0, 0, 0);
            
        [unroll(30)]
        for (int j = 0; j < _Iteration; j++)
        {
            acumulateColor += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
            i.uv.xy += blurVector;
        }
            
        return acumulateColor / _Iteration;
    }

    half4 Frag(VaryingsDefault input): SV_Target
    {
        float2 uv0 = input.uv - _RadialCenter;
        float theta = atan2(uv0.y, uv0.x);
        theta = theta * INV_PI * 0.5 + 0.5;
        float r = length(uv0);

        float2 polarUV = float2(theta, r) + _Time.y * NoiseTexValue.zw; //zw speed
        float4 noiseTex = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, float2(polarUV.x * NoiseTexValue.x,polarUV.y * NoiseTexValue.y)); //xy == tilling

        float4 c = RadialBlur(input);
        c.rgb = lerp(luminance(c.rgb),c.rgb,_Params1.y);
        c.rgb = lerp(half3(0.5,0.5,0.5),c.rgb,_Params1.z);
//return c;
        //c = saturate(1-c);
        c*=noiseTex.r;
        c = step(c.r, _Params1.x);
        c*=_Color;
        c.rgb = lerp(c.rgb, 1 - c.rgb,_Params1.w);
        return c;

        

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