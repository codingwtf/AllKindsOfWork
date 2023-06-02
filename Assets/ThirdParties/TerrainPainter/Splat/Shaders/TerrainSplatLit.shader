Shader "TerrainSplat/URP/Lit"
{
    Properties
    {
        [CompactTextureDrawer()]_MainTexArray ("Albedo Smoothness Array", 2DArray) = "white" {}
        //[CompactTextureDrawer()]_MainTexAtlas ("Albedo Smoothness Atlas", 2DArray) = "white" {}
        [CompactTextureDrawer()]_NormalTexArray ("Normal Array", 2DArray) = "white" {}
        //[CompactTextureDrawer()]_NormalTexAtlas ("Normal Atlas", 2DArray) = "white" {}
        _IndexTex ("Index Map", 2D) = "white" {}
        _WeightTex ("Weight Map", 2D) = "white" {}
        _Weight ("Weight", Range(0.01, 1)) = 0.5
		[KeywordEnum(X2, X3)] _SPLAT ("Splat", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        LOD 100
        
        HLSLINCLUDE
        //#define _NORMALMAP
        #define _NORMAL_DROPOFF_TS
        //#define _USEATLAS
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "TerrainSplat.hlsl"

        //这边就是定义了纹理数组的使用
        DECLARE_TEX_SET(_MainTex);
        DECLARE_TEX_SET(_NormalTex);
        float _Weight;
        //SurfaceData
        SurfaceData Surf(float2 uv)
        {
            // 权重可以使用线性采样
            half3 weight = SAMPLE_TEXTURE2D(_WeightTex, sampler_linear_clamp, uv).rgb;
            //#define DecodeIndex(idx) floor((idx) * 128 + 0.5)
            // 索引只能使用点采样模式
            uint3 index = DecodeIndex(SAMPLE_TEXTURE2D(_IndexTex, sampler_point_clamp, uv).rgb);
            
            //传入索引和UV
            float4 uv1 = GetUVByIndex(float3(uv, index.r));
            float4 uv2 = GetUVByIndex(float3(uv, index.g));
            float4 uv3 = GetUVByIndex(float3(uv, index.b));
            
            half4 albedoSmoothness1 = SAMPLE_TEX_SET(_MainTex, uv1);
            half4 albedoSmoothness2 = SAMPLE_TEX_SET(_MainTex, uv2);
            half4 albedoSmoothness3 = SAMPLE_TEX_SET(_MainTex, uv3);
            
            float3 normal1 = UnpackNormal(SAMPLE_TEX_SET(_NormalTex, uv1));
            float3 normal2 = UnpackNormal(SAMPLE_TEX_SET(_NormalTex, uv2));
            float3 normal3 = UnpackNormal(SAMPLE_TEX_SET(_NormalTex, uv3));
        #ifdef _SPLAT_X2
            float2 blend = blend_height(albedoSmoothness1.a, albedoSmoothness2.a, weight.rg, _Weight);
            //half4 albedoSmoothness = lerp(albedoSmoothness2, albedoSmoothness1, weight.r);
            half4 albedoSmoothness = blend.r * albedoSmoothness1 + blend.g * albedoSmoothness2;
            half3 normal = blend_whiteoutW(normal2, normal1, weight.r);
        #else
            float3 blend = blend_height(albedoSmoothness1.a, albedoSmoothness2.a, albedoSmoothness2.a,weight.rgb, _Weight);
            //half4 albedoSmoothness = albedoSmoothness1 * weight.r + albedoSmoothness2 * weight.g + albedoSmoothness3 * weight.b;
            half4 albedoSmoothness = albedoSmoothness1 * blend.r + albedoSmoothness2 * blend.g + albedoSmoothness3 * blend.b;
            float3 normal = blend_whiteoutW(normal1, normal2, normal3, weight.r, weight.g, weight.b);
        #endif
            SurfaceData o;
            o.albedo = albedoSmoothness.rgb;
            o.normalTS = normal;
            o.specular = 0;
            o.smoothness = 0.5;//albedoSmoothness.a;
            o.metallic = 0;
            o.alpha = 1;
            o.emission = 0;
            o.occlusion = 1.0;
            return o;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _SPLAT_X2 _SPLAT_X3
            #pragma shader_feature_local _NORMALMAP
            //#pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _EMISSION
            //#pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            //#pragma shader_feature_local_fragment _OCCLUSIONMAP
            //#pragma shader_feature_local _PARALLAXMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            
            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 texcoord     : TEXCOORD0;
                float2 lightmapUV   : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float2 uv                       : TEXCOORD0;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
            
            #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
                float3 positionWS               : TEXCOORD2;
            #endif
            
            #ifdef _NORMALMAP
                float4 normalWS                 : TEXCOORD3;    // xyz: normal, w: viewDir.x
                float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: viewDir.y
                float4 bitangentWS              : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
            #else
                float3 normalWS                 : TEXCOORD3;
                float3 viewDirWS                : TEXCOORD4;
            #endif
            
                half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light
            
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                float4 shadowCoord              : TEXCOORD7;
            #endif
            
                float4 positionCS               : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
            {
                inputData = (InputData)0;
            
            #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
                inputData.positionWS = input.positionWS;
            #endif
            
            #ifdef _NORMALMAP
                half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
                inputData.normalWS = TransformTangentToWorld(normalTS,
                    half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz));
            #else
                half3 viewDirWS = input.viewDirWS;
                inputData.normalWS = input.normalWS;
            #endif
            
                inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
                viewDirWS = SafeNormalize(viewDirWS);
                inputData.viewDirectionWS = viewDirWS;
            
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                inputData.shadowCoord = input.shadowCoord;
            #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
            #else
                inputData.shadowCoord = float4(0, 0, 0, 0);
            #endif
            
                inputData.fogCoord = input.fogFactorAndVertexLight.x;
                inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
                inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
            
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
            
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
                half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
                half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
            
                output.uv = input.texcoord;
            
            #ifdef _NORMALMAP
                output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
                output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
                output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
            #else
                output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
                output.viewDirWS = viewDirWS;
            #endif
            
                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
            
                output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
            
            #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
                output.positionWS = vertexInput.positionWS;
            #endif
            
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                output.shadowCoord = GetShadowCoord(vertexInput);
            #endif
            
                output.positionCS = vertexInput.positionCS;
            
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                SurfaceData surfaceData;
                surfaceData = Surf(input.uv);

                InputData inputData;
                InitializeInputData(input, surfaceData.normalTS, inputData);
            
                //half4 color = UniversalFragmentPBR(inputData, surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.occlusion, surfaceData.emission, surfaceData.alpha);
                //half4 color = UniversalFragmentPBR(inputData, surfaceData);
                half4 color = UniversalFragmentBlinnPhong(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return color;

            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}
        
            ZWrite On
            ZTest LEqual
            Cull Back
        
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
        
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
        
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
        
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}
        
            ZWrite On
            ColorMask 0
            Cull[_Cull]
        
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
        
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
        
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
        
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
        
        // This pass it not used during regular rendering, only for lightmap baking.
        /*Pass
        {
            Name "Meta"
            Tags{"LightMode" = "Meta"}
        
            Cull Off
        
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
        
            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMeta
        
            #pragma shader_feature _SPECULAR_SETUP
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICSPECGLOSSMAP
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        
            #pragma shader_feature _SPECGLOSSMAP
        
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitMetaPass.hlsl"
        
            ENDHLSL
        }*/
    }
}
