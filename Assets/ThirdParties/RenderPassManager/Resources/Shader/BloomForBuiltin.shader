Shader "Unlit/BloomForBuiltin"
{

HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
        TEXTURE2D_X(_SourceTex);
        float4 _SourceTex_TexelSize;
        TEXTURE2D_X(_SourceTexLowMip);
        float4 _SourceTexLowMip_TexelSize;
    uniform half4 _BloomForBuiltinParams;       // x: scatter, y: clamp, z: threshold (linear), w: threshold knee
    #define Scatter             _BloomForBuiltinParams.x
    #define ClampMax            _BloomForBuiltinParams.y
    #define Threshold           _BloomForBuiltinParams.z
    #define ThresholdKnee       _BloomForBuiltinParams.w
    

 
	half4 PrefilterFrag(Varyings input) : SV_Target
	{
	    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);
		half4 color = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv);
		
		//bloom计算在linearspace
		color.rgb = Gamma22ToLinear(color.rgb);

	    half brightness = max(color.r,max(color.g,color.b));
        half softness = clamp(brightness - Threshold + ThresholdKnee, 0.0, 2.0 * ThresholdKnee);
        softness = (softness * softness) / (4.0 * ThresholdKnee + 1e-4);
        half multiplier = max(brightness - Threshold, softness) / max(brightness, 1e-4);
        color *= multiplier;

        color = max(color, 0);
		return color;
	}
	
        half4 FragBlurH(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float texelSize = _SourceTex_TexelSize.x * 2.0;
            float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);

            // 9-tap gaussian blur on the downsampled source
            half3 c0 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv - float2(texelSize * 4.0, 0.0));
            half3 c1 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv - float2(texelSize * 3.0, 0.0));
            half3 c2 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv - float2(texelSize * 2.0, 0.0));
            half3 c3 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv - float2(texelSize * 1.0, 0.0));
            half3 c4 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv                               );
            half3 c5 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + float2(texelSize * 1.0, 0.0));
            half3 c6 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + float2(texelSize * 2.0, 0.0));
            half3 c7 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + float2(texelSize * 3.0, 0.0));
            half3 c8 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + float2(texelSize * 4.0, 0.0));

            half3 color = c0 * 0.01621622 + c1 * 0.05405405 + c2 * 0.12162162 + c3 * 0.19459459
                        + c4 * 0.22702703
                        + c5 * 0.19459459 + c6 * 0.12162162 + c7 * 0.05405405 + c8 * 0.01621622;

            return half4(color,1.0);
        }

        half4 FragBlurV(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float texelSize = _SourceTex_TexelSize.y;
            float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);

            // Optimized bilinear 5-tap gaussian on the same-sized source (9-tap equivalent)
            half3 c0 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv - float2(0.0, texelSize * 3.23076923));
            half3 c1 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv - float2(0.0, texelSize * 1.38461538));
            half3 c2 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv                                      );
            half3 c3 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + float2(0.0, texelSize * 1.38461538));
            half3 c4 = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + float2(0.0, texelSize * 3.23076923));

            half3 color = c0 * 0.07027027 + c1 * 0.31621622
                        + c2 * 0.22702703
                        + c3 * 0.31621622 + c4 * 0.07027027;

            return half4(color,1.0);
        }
        
        half3 Upsample(float2 uv)
        {
            half3 highMip = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv);
            half3 lowMip = SAMPLE_TEXTURE2D_X(_SourceTexLowMip, sampler_LinearClamp, uv);
            return lerp(highMip, lowMip, Scatter);
        }
        
        half4 FragUpsample(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            half3 color = Upsample(UnityStereoTransformScreenSpaceTex(input.uv));
            return half4(color,1.0);
        }
        
        half4 FragFinal(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            half3 color = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, input.uv);
            //color = LinearToGamma22(color);
            return half4(color,0.0);
        }
        
        half Random_Vec2D(half2 Vec)
{
    half2 temp = Vec;
    half x = dot( temp * half2(12.456,21.654),half2(45.789,87.654));
    half y = dot( temp * half2(45.789,87.654),half2(12.456,21.654));
    return frac( dot(half2(x,y) , Vec) );
}

half2 RotVec2D(half2 Vec,half Angle)
{
    half2x2 mat = half2x2(cos(Angle),-sin(Angle),sin(Angle),cos(Angle));
    return mul(mat,Vec);
}

        half3 Sample4XPoint(float2 uv)
        {
            const half FILTER_SIZE = 1.0;
            float offset = _SourceTex_TexelSize.x * FILTER_SIZE;
            const half TO_ANGLE = 3600;
            half r = Random_Vec2D(uv) * TO_ANGLE;
            
            half3 c0 = (SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + RotVec2D(float2(-offset,-offset),r)));
            half3 c1 = (SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + RotVec2D(float2(-offset,offset),r)));
            half3 c2 = (SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + RotVec2D(float2(offset,-offset),r)));
            half3 c3 = (SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + RotVec2D(float2(offset,offset),r)));
            
            return (c0 + c1 + c2 + c3)*0.25;
        }
        
        half4 FragBlur4XH(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);
            float offset = _SourceTex_TexelSize.x * 4;
            half3 result = Sample4XPoint(uv) + Sample4XPoint(uv + float2(-offset,0)) * 0.5 + Sample4XPoint(uv + float2(offset,0)) * 0.5;
            return  half4(result *0.75,1.0) ;
        }

        half4 FragBlur4XV(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);
            float offset = _SourceTex_TexelSize.y;
            const real TO_ANGLE = 3600;
            real r = Random_Vec2D(uv) * TO_ANGLE;
            half3 c0 = (SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + RotVec2D(float2(0,-offset),r))) * 0.3;
            half3 c1 = (SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv ));
            half3 c2 = (SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv + RotVec2D(float2(0,offset),r))) * 0.3;
            return  half4((c0+c1+c2) *1.3,1.0) ;
        }
	ENDHLSL
	
    SubShader
	{
	    Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always ZWrite Off Cull Off
		Pass
		{
		    Name "Bloom Prefilter"
			HLSLPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma vertex FullscreenVert
			#pragma fragment PrefilterFrag 
			ENDHLSL
		}
		
       Pass
        {
            Name "Bloom Blur Horizontal"

            HLSLPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest 
                #pragma vertex FullscreenVert
                #pragma fragment FragBlurH
            ENDHLSL
        }

        Pass
        {
            Name "Bloom Blur Vertical"

            HLSLPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest 
                #pragma vertex FullscreenVert
                #pragma fragment FragBlurV
            ENDHLSL
        }

        Pass
        {
            Name "Bloom Upsample"

            HLSLPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest 
                #pragma vertex FullscreenVert
                #pragma fragment FragUpsample
            ENDHLSL
        }
        
        Pass
        {
            Name "Bloom Blit"
            Blend One One
            HLSLPROGRAM
            #pragma fragmentoption ARB_precision_hint_fastest 
                #pragma vertex FullscreenVert
                #pragma fragment FragFinal
            ENDHLSL
        }
        
        Pass
        {
            Name "Bloom Blur4XH"
            HLSLPROGRAM
                #pragma vertex FullscreenVert
                #pragma fragment FragBlur4XH
            ENDHLSL
        }
        
        Pass
        {
            Name "Bloom Blur4XV"
            HLSLPROGRAM
                #pragma vertex FullscreenVert
                #pragma fragment FragBlur4XV
            ENDHLSL
        }
	}
}
