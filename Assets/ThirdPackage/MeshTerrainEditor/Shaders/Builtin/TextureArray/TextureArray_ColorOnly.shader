// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MTE/Standard/TextureArray_ColorOnly"
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
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma shader_feature ENABLE_LAYER_UV_SCALE
		#pragma shader_feature_local _HasWeightMap1
		#pragma shader_feature_local _HasWeightMap2
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


		inline float MipMapLevel10_g28( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel10_g25( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel10_g26( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel10_g27( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		float4 WeightedBlend419_g24( half4 Weight, float4 Layer1, float4 Layer2, float4 Layer3, float4 Layer4 )
		{
			return Layer1 * Weight.r + Layer2 * Weight.g + Layer3 * Weight.b + Layer4 * Weight.a;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 localGetMax4WeightLayers25_g24 = ( float4( 0,0,0,0 ) );
			float2 uv_Control0 = i.uv_texcoord * _Control0_ST.xy + _Control0_ST.zw;
			float4 Control025_g24 = tex2D( _Control0, uv_Control0 );
			float4 _Vector0 = float4(0,0,0,0);
			float2 uv_Control1 = i.uv_texcoord * _Control1_ST.xy + _Control1_ST.zw;
			#ifdef _HasWeightMap1
				float4 staticSwitch12_g24 = tex2D( _Control1, uv_Control1 );
			#else
				float4 staticSwitch12_g24 = _Vector0;
			#endif
			float4 Control125_g24 = staticSwitch12_g24;
			float2 uv_Control2 = i.uv_texcoord * _Control2_ST.xy + _Control2_ST.zw;
			#ifdef _HasWeightMap2
				float4 staticSwitch11_g24 = tex2D( _Control2, uv_Control2 );
			#else
				float4 staticSwitch11_g24 = _Vector0;
			#endif
			float4 Control225_g24 = staticSwitch11_g24;
			float4 Weights25_g24 = float4( 1,0,0,0 );
			float4 Indices25_g24 = float4( 1,0,0,0 );
			{
			Max4WeightLayer(Control025_g24, Control125_g24, Control225_g24, Weights25_g24, Indices25_g24);
			}
			float4 LayerWeights61_g24 = Weights25_g24;
			float4 Weight19_g24 = LayerWeights61_g24;
			float4 break26_g24 = Indices25_g24;
			int temp_output_7_0_g28 = (int)break26_g24.x;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g28 = LayerUVScales[temp_output_7_0_g28];
			#else
				float staticSwitch15_g28 = _UVScale;
			#endif
			float2 temp_output_14_0_g28 = ( i.uv_texcoord * staticSwitch15_g28 );
			float uvInTexel10_g28 = ( temp_output_14_0_g28 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g28 = MipMapLevel10_g28( uvInTexel10_g28 );
			float4 Layer119_g24 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g28,(float)temp_output_7_0_g28), localMipMapLevel10_g28 );
			int temp_output_7_0_g25 = (int)break26_g24.y;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g25 = LayerUVScales[temp_output_7_0_g25];
			#else
				float staticSwitch15_g25 = _UVScale;
			#endif
			float2 temp_output_14_0_g25 = ( i.uv_texcoord * staticSwitch15_g25 );
			float uvInTexel10_g25 = ( temp_output_14_0_g25 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g25 = MipMapLevel10_g25( uvInTexel10_g25 );
			float4 Layer219_g24 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g25,(float)temp_output_7_0_g25), localMipMapLevel10_g25 );
			int temp_output_7_0_g26 = (int)break26_g24.z;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g26 = LayerUVScales[temp_output_7_0_g26];
			#else
				float staticSwitch15_g26 = _UVScale;
			#endif
			float2 temp_output_14_0_g26 = ( i.uv_texcoord * staticSwitch15_g26 );
			float uvInTexel10_g26 = ( temp_output_14_0_g26 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g26 = MipMapLevel10_g26( uvInTexel10_g26 );
			float4 Layer319_g24 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g26,(float)temp_output_7_0_g26), localMipMapLevel10_g26 );
			int temp_output_7_0_g27 = (int)break26_g24.w;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g27 = LayerUVScales[temp_output_7_0_g27];
			#else
				float staticSwitch15_g27 = _UVScale;
			#endif
			float2 temp_output_14_0_g27 = ( i.uv_texcoord * staticSwitch15_g27 );
			float uvInTexel10_g27 = ( temp_output_14_0_g27 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g27 = MipMapLevel10_g27( uvInTexel10_g27 );
			float4 Layer419_g24 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g27,(float)temp_output_7_0_g27), localMipMapLevel10_g27 );
			float4 localWeightedBlend419_g24 = WeightedBlend419_g24( Weight19_g24 , Layer119_g24 , Layer219_g24 , Layer319_g24 , Layer419_g24 );
			o.Albedo = (localWeightedBlend419_g24).xyz;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "MTE.MTEColorOnlyTextureArrayShaderGUI"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;234;1392.133,-652.0233;Float;False;True;-1;2;MTE.MTEColorOnlyTextureArrayShaderGUI;0;0;Standard;MTE/Standard/TextureArray_ColorOnly;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;1;Include;../MTECommonBuiltinRP.hlsl;False;;Custom;False;0;0;;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.FunctionNode;237;1025.352,-650.5337;Inherit;False;MTE_BuiltinRP_TextureArrayCore_Color;0;;24;9665015b962c47646885fa699f9af9c8;0;0;1;FLOAT3;0
WireConnection;234;0;237;0
ASEEND*/
//CHKSM=A17522510CB3D1A9154AD319809E4E750B8B7E0C