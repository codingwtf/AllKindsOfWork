Shader "Urp/PlaneReflectionTexture"
{
    Properties
    {
        _Alpha ("Alpha", range(0, 1)) = 0.8
    	
    	[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendRGB ("BlendSrcRGB", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendRGB ("BlendDstRGB", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent"
            "Queue"="Transparent+0"}
        LOD 100
        Pass
        {
			Name "CharacterReflection"
			Tags{"LightMode" = "UniversalForward"}
        	Cull back
            ZWrite off
            Blend[_SrcBlendRGB][_DstBlendRGB]
        	
			HLSLPROGRAM
            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram            
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct VertexInput
            {
                float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
            };

            struct VertexOutput
            {
				float4 positionCS : SV_POSITION;
				float3 localNormal : TEXCOORD0;
				float4 screenPosition : TEXCOORD1;
            };

			TEXTURE2D(_PlanarReflectionTexture); SAMPLER(sampler_PlanarReflectionTexture);
			
			float _Alpha;

            VertexOutput VertexProgram(VertexInput input)
            {
				VertexOutput output = (VertexOutput)0;
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.screenPosition = ComputeScreenPos(output.positionCS);
                return output;
            }

			half4 FragmentProgram(VertexOutput input) : SV_Target
			{
				float2 uv = input.screenPosition.xy / input.screenPosition.w;
				half4 col = SAMPLE_TEXTURE2D(_PlanarReflectionTexture, sampler_PlanarReflectionTexture,uv);
				col.rgb = col.rgb;
				col.a = _Alpha * smoothstep(0,0.25,uv.y);
				return col;
            }
			ENDHLSL
        }
    }
}