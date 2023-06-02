// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MTE/Standard/TextureArray_ColorAndNormal"
{
	Properties
	{
		_Control0("Control0", 2D) = "white" {}
		_TextureArray0("TextureArray0", 2DArray) = "white" {}
		[Toggle(ENABLE_LAYER_UV_SCALE)] ENABLE_LAYER_UV_SCALE("Enable per-layer UV Scale", Float) = 1
		_UVScale("UV Scale", Range( 0.01 , 100)) = 1
		_Control1("Control1", 2D) = "white" {}
		_Control2("Control2", 2D) = "white" {}
		[Toggle(ENABLE_LAYER_UV_SCALE)] ENABLE_LAYER_UV_SCALE1("Enable per-layer UV Scale", Float) = 1
		_NormalIntensity("Normal Intensity", Range( 0.01 , 10)) = 1
		[Toggle(ENABLE_NORMAL_INTENSITY)] ENABLE_NORMAL_INTENSITY("Enable Normal Intensity", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma shader_feature ENABLE_LAYER_UV_SCALE
		#pragma shader_feature_local _HasWeightMap1
		#pragma shader_feature_local _HasWeightMap2
		#pragma shader_feature ENABLE_NORMAL_INTENSITY
		#include "../MTECommonBuiltinRP.hlsl"
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D_ARRAY_LOD(tex,samplerTex,coord,lod) tex.SampleLevel(samplerTex,coord, lod)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D_ARRAY_LOD(tex,samplertex,coord,lod) tex2DArraylod(tex, float4(coord,lod))
		#endif//ASE Sampling Macros

		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(_TextureArray0);
		uniform float _UVScale;
		uniform float LayerUVScales[12];
		uniform sampler2D _Control0;
		uniform float4 _Control0_ST;
		uniform sampler2D _Control1;
		uniform float4 _Control1_ST;
		uniform sampler2D _Control2;
		uniform float4 _Control2_ST;
		float4 _TextureArray0_TexelSize;
		SamplerState sampler_TextureArray0;
		uniform float _NormalIntensity;


		inline float MipMapLevel42_g74( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel42_g71( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel42_g72( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel42_g73( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		float4 WeightedBlend417_g29( half4 Weight, float4 Layer1, float4 Layer2, float4 Layer3, float4 Layer4 )
		{
			return Layer1 * Weight.r + Layer2 * Weight.g + Layer3 * Weight.b + Layer4 * Weight.a;
		}


		inline float MipMapLevel10_g63( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel10_g64( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel10_g65( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel10_g66( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		float4 WeightedBlend419_g29( half4 Weight, float4 Layer1, float4 Layer2, float4 Layer3, float4 Layer4 )
		{
			return Layer1 * Weight.r + Layer2 * Weight.g + Layer3 * Weight.b + Layer4 * Weight.a;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 localGetMax4WeightLayers25_g29 = ( float4( 0,0,0,0 ) );
			float2 uv_Control0 = i.uv_texcoord * _Control0_ST.xy + _Control0_ST.zw;
			float4 Control025_g29 = tex2D( _Control0, uv_Control0 );
			float4 _Vector0 = float4(0,0,0,0);
			float2 uv_Control1 = i.uv_texcoord * _Control1_ST.xy + _Control1_ST.zw;
			#ifdef _HasWeightMap1
				float4 staticSwitch12_g29 = tex2D( _Control1, uv_Control1 );
			#else
				float4 staticSwitch12_g29 = _Vector0;
			#endif
			float4 Control125_g29 = staticSwitch12_g29;
			float2 uv_Control2 = i.uv_texcoord * _Control2_ST.xy + _Control2_ST.zw;
			#ifdef _HasWeightMap2
				float4 staticSwitch11_g29 = tex2D( _Control2, uv_Control2 );
			#else
				float4 staticSwitch11_g29 = _Vector0;
			#endif
			float4 Control225_g29 = staticSwitch11_g29;
			float4 Weights25_g29 = float4( 1,0,0,0 );
			float4 Indices25_g29 = float4( 1,0,0,0 );
			{
			Max4WeightLayer(Control025_g29, Control125_g29, Control225_g29, Weights25_g29, Indices25_g29);
			}
			float4 LayerWeights61_g29 = Weights25_g29;
			float4 Weight17_g29 = LayerWeights61_g29;
			float4 break26_g29 = Indices25_g29;
			int temp_output_38_0_g74 = (int)break26_g29.x;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g74 = LayerUVScales[temp_output_38_0_g74];
			#else
				float staticSwitch40_g74 = _UVScale;
			#endif
			float2 temp_output_34_0_g74 = ( i.uv_texcoord * staticSwitch40_g74 );
			float uvInTexel42_g74 = ( temp_output_34_0_g74 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel42_g74 = MipMapLevel42_g74( uvInTexel42_g74 );
			float4 Layer117_g29 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_34_0_g74,(float)temp_output_38_0_g74), localMipMapLevel42_g74 );
			int temp_output_38_0_g71 = (int)break26_g29.y;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g71 = LayerUVScales[temp_output_38_0_g71];
			#else
				float staticSwitch40_g71 = _UVScale;
			#endif
			float2 temp_output_34_0_g71 = ( i.uv_texcoord * staticSwitch40_g71 );
			float uvInTexel42_g71 = ( temp_output_34_0_g71 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel42_g71 = MipMapLevel42_g71( uvInTexel42_g71 );
			float4 Layer217_g29 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_34_0_g71,(float)temp_output_38_0_g71), localMipMapLevel42_g71 );
			int temp_output_38_0_g72 = (int)break26_g29.z;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g72 = LayerUVScales[temp_output_38_0_g72];
			#else
				float staticSwitch40_g72 = _UVScale;
			#endif
			float2 temp_output_34_0_g72 = ( i.uv_texcoord * staticSwitch40_g72 );
			float uvInTexel42_g72 = ( temp_output_34_0_g72 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel42_g72 = MipMapLevel42_g72( uvInTexel42_g72 );
			float4 Layer317_g29 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_34_0_g72,(float)temp_output_38_0_g72), localMipMapLevel42_g72 );
			int temp_output_38_0_g73 = (int)break26_g29.w;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g73 = LayerUVScales[temp_output_38_0_g73];
			#else
				float staticSwitch40_g73 = _UVScale;
			#endif
			float2 temp_output_34_0_g73 = ( i.uv_texcoord * staticSwitch40_g73 );
			float uvInTexel42_g73 = ( temp_output_34_0_g73 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel42_g73 = MipMapLevel42_g73( uvInTexel42_g73 );
			float4 Layer417_g29 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_34_0_g73,(float)temp_output_38_0_g73), localMipMapLevel42_g73 );
			float4 localWeightedBlend417_g29 = WeightedBlend417_g29( Weight17_g29 , Layer117_g29 , Layer217_g29 , Layer317_g29 , Layer417_g29 );
			#ifdef ENABLE_NORMAL_INTENSITY
				float staticSwitch106_g29 = _NormalIntensity;
			#else
				float staticSwitch106_g29 = 1.0;
			#endif
			o.Normal = UnpackScaleNormal( localWeightedBlend417_g29, staticSwitch106_g29 );
			float4 Weight19_g29 = LayerWeights61_g29;
			int temp_output_7_0_g63 = (int)break26_g29.x;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g63 = LayerUVScales[temp_output_7_0_g63];
			#else
				float staticSwitch15_g63 = _UVScale;
			#endif
			float2 temp_output_14_0_g63 = ( i.uv_texcoord * staticSwitch15_g63 );
			float uvInTexel10_g63 = ( temp_output_14_0_g63 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g63 = MipMapLevel10_g63( uvInTexel10_g63 );
			float4 Layer119_g29 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g63,(float)temp_output_7_0_g63), localMipMapLevel10_g63 );
			int temp_output_7_0_g64 = (int)break26_g29.y;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g64 = LayerUVScales[temp_output_7_0_g64];
			#else
				float staticSwitch15_g64 = _UVScale;
			#endif
			float2 temp_output_14_0_g64 = ( i.uv_texcoord * staticSwitch15_g64 );
			float uvInTexel10_g64 = ( temp_output_14_0_g64 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g64 = MipMapLevel10_g64( uvInTexel10_g64 );
			float4 Layer219_g29 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g64,(float)temp_output_7_0_g64), localMipMapLevel10_g64 );
			int temp_output_7_0_g65 = (int)break26_g29.z;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g65 = LayerUVScales[temp_output_7_0_g65];
			#else
				float staticSwitch15_g65 = _UVScale;
			#endif
			float2 temp_output_14_0_g65 = ( i.uv_texcoord * staticSwitch15_g65 );
			float uvInTexel10_g65 = ( temp_output_14_0_g65 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g65 = MipMapLevel10_g65( uvInTexel10_g65 );
			float4 Layer319_g29 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g65,(float)temp_output_7_0_g65), localMipMapLevel10_g65 );
			int temp_output_7_0_g66 = (int)break26_g29.w;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g66 = LayerUVScales[temp_output_7_0_g66];
			#else
				float staticSwitch15_g66 = _UVScale;
			#endif
			float2 temp_output_14_0_g66 = ( i.uv_texcoord * staticSwitch15_g66 );
			float uvInTexel10_g66 = ( temp_output_14_0_g66 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g66 = MipMapLevel10_g66( uvInTexel10_g66 );
			float4 Layer419_g29 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g66,(float)temp_output_7_0_g66), localMipMapLevel10_g66 );
			float4 localWeightedBlend419_g29 = WeightedBlend419_g29( Weight19_g29 , Layer119_g29 , Layer219_g29 , Layer319_g29 , Layer419_g29 );
			o.Albedo = (localWeightedBlend419_g29).xyz;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Hidden/InternalErrorShader"
	CustomEditor "MTE.MTEColorAndNormalTextureArrayShaderGUI"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;232;1729.133,-654.0233;Float;False;True;-1;2;MTE.MTEColorAndNormalTextureArrayShaderGUI;0;0;Standard;MTE/Standard/TextureArray_ColorAndNormal;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;Hidden/InternalErrorShader;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;1;Include;../MTECommonBuiltinRP.hlsl;False;;Custom;False;0;0;;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.FunctionNode;234;1293.311,-654.4985;Inherit;False;MTE_BuiltinRP_TextureArrayCore_ColorAndNormal;0;;29;fbea9b15efa251f42a4d93433849dcae;0;0;2;FLOAT3;0;FLOAT3;93
WireConnection;232;0;234;0
WireConnection;232;1;234;93
ASEEND*/
//CHKSM=3DFBC1940E9EDBAE85B01C3ADAFE11C40F62D71F