Shader "Shading/PBR/MyPBR"
{
    Properties
    {
        _Color("Color(RGB)",Color) = (1,1,1,1)
        _MainTex("MainTex",2D) = "gary"{}
        [Gamma] _Metallic("Metallic", range(0,1)) = 0
        _Smoothness("Smoothness", range(0,1)) = 0.5
        _Lut("Lut Tex", 2D) = "White"{}
    }
    SubShader
    {
        Tags
        {

            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry+0"
        }
        
        Pass
        {
            Name "PBR Shading"
            Tags 
            { 
                
            }
            
            Cull Back
            ZWrite On
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Metallic;
                half _Smoothness;
            CBUFFER_END
            
            
            TEXTURE2D(_MainTex);
            float4 _MainTex_ST;
            TEXTURE2D(_Lut);
            float4 _Lut_ST;
            
            #define smp SamplerState_Linear_Repeat
            SAMPLER(smp);
            
            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv :TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv :TEXCOORD0;
                float3 normalWS:TEXCOORD1;
                float3 positionWS:TEXCOORD2;
            };
            
            
            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionWS = TransformObjectToWorld(v.positionOS);
                o.normalWS = TransformObjectToWorldNormal(v.normal, true);
                return o;
            }

            // a 表面粗糙度，是perceptualRoughness * perceptualRoughness 即 roughness
            // unity 将 a lerp 到 0.002 到1
            inline float D_GGX_TR(float nh, float a)
            {
                a = lerp(0.002, 1, a);
                float a2 = a * a;

                float demon = nh * nh * (a2 - 1) + 1;
                demon = PI * demon * demon; 
                return a2 / demon;
            }

            inline float GeometrySchlickGGX(float nox, float k)
            {
                return nox / (nox * (1 - k) + k);
            }
            
            inline float GeometrySmith(float nv, float nl, float k)
            {
                return GeometrySchlickGGX(nv, k) * GeometrySchlickGGX(nl, k);
            }

            inline float3 fresnelSchlickRoughness(float cosTheta, float3 F0, float roughness)
            {
	            return F0 + (max(float3(1.0 - roughness, 1.0 - roughness, 1.0 - roughness), F0) - F0) * pow(1.0 - cosTheta, 5.0);
            }
            
            half4 frag(Varyings i) : SV_TARGET 
            {
                float3 normalWS = normalize(i.normalWS);
                float3 viewDir = GetCameraPositionWS() - i.positionWS;
                float3 lightDir = _MainLightPosition;
                float3 halfVector = normalize(viewDir + lightDir);
                half3 lightColor = _MainLightColor;
                
                float perceptualRoughness = 1 - _Smoothness;
                float roughness = perceptualRoughness * perceptualRoughness;
                float squareRoughness = roughness * roughness;

                float nl = max(saturate(dot(normalWS, lightDir)), 0.0001);
                float nv = max(saturate(dot(normalWS, viewDir)), 0.0001);
                float nh = max(saturate(dot(normalWS, halfVector)), 0.0001);
                float vh = max(saturate(dot(viewDir, halfVector)), 0.0001);
                float lh = max(saturate(dot(lightDir, halfVector)), 0.0001);

                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex,smp,i.uv);
                half3 albedo = _Color * mainTex;

                float D = D_GGX_TR(nh, roughness);
                //  G k -- direct is (a + 1)2 / 8
                //  G k -- ibl is (a)2 / 2
                float Kdirect_G = pow(roughness + 1,2) / 8;
                float G = GeometrySmith(nv, nl, Kdirect_G);

                float3 f0 = lerp(half3(0.04,0.04,0.04), albedo, _Metallic);
                // F : unreal 使用这个拟合 计算优化
                float3 F = f0 + (1 - f0) * exp2((-5.55473 * vh - 6.98316) * vh);
                //float3 F = f0 + (1 - f0) * pow(1 - vh, 5);
                
                // Ks 就是F，已经放到F计算了
                half3 directSpecCol =((D * F * G * 0.25) / (nl * nv)) * lightColor * nl * PI; // *PI 因为 diffuse 没有除以 PI，这边× 守恒？

                // albedo / pi ?? unity not divive PI
                //half3 directDiffuseCol = Kd * albedo / PI * lightColor * nl;
                float Kd = (1 - F) * (1 - _Metallic); // *(1 - _Metallic) 表示全金属的话没有漫反射了
                half3 directDiffuseCol = Kd * albedo * lightColor * nl;


                half3 iblDiffuse = SampleSH(normalWS);
                

                //PerceptualRoughnessToMipmapLevel(real perceptualRoughness, real NdotR)
                // other
                half mipLevel = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
                float reflectDir = reflect(-viewDir, normalWS);
                half4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectDir, mipLevel);
                half3 iblSpecular = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);

                float2 envBDRF = SAMPLE_TEXTURE2D(_Lut, smp,float2(lerp(0, 0.99, nv), lerp(0, 0.99, roughness))).rg;
                float3 Flast = fresnelSchlickRoughness(max(nv, 0.0), f0, roughness);

                Kd = (1 - Flast) * (1 - _Metallic);
                half3 iblDiffuseCol = Kd * iblDiffuse * albedo;
                
                half3 iblSpecCol = iblSpecular * (Flast * envBDRF.r + envBDRF.g);
                
                half3 resultDirect = directDiffuseCol + directSpecCol;
                half3 resultIndirect = iblDiffuseCol + iblSpecCol;

                half4 resultColor = half4(resultDirect + resultIndirect, 1);
                
                return resultColor;
            }
            
            ENDHLSL
        }
    }
}