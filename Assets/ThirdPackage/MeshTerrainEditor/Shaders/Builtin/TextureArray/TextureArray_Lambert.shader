// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MTE/Surface/TextureArray/Lambert"
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
		#pragma target 3.5
		#pragma shader_feature ENABLE_LAYER_UV_SCALE
		#pragma shader_feature_local _HasWeightMap1
		#pragma shader_feature_local _HasWeightMap2
		#include "../MTECommonBuiltinRP.hlsl"
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D_ARRAY_LOD(tex,samplerTex,coord,lod) tex.SampleLevel(samplerTex,coord, lod)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D_ARRAY_LOD(tex,samplertex,coord,lod) tex2DArraylod(tex, float4(coord,lod))
		#endif//ASE Sampling Macros

		#pragma surface surf Lambert keepalpha addshadow fullforwardshadows 
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


		inline half MipMapLevel10_g25( half uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline half MipMapLevel10_g22( half uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline half MipMapLevel10_g23( half uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline half MipMapLevel10_g24( half uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		float4 WeightedBlend419_g19( half4 Weight, float4 Layer1, float4 Layer2, float4 Layer3, float4 Layer4 )
		{
			return Layer1 * Weight.r + Layer2 * Weight.g + Layer3 * Weight.b + Layer4 * Weight.a;
		}


		void surf( Input i , inout SurfaceOutput o )
		{
			half4 localGetMax4WeightLayers25_g19 = ( float4( 0,0,0,0 ) );
			float2 uv_Control0 = i.uv_texcoord * _Control0_ST.xy + _Control0_ST.zw;
			half4 Control025_g19 = tex2D( _Control0, uv_Control0 );
			half4 _Vector0 = half4(0,0,0,0);
			float2 uv_Control1 = i.uv_texcoord * _Control1_ST.xy + _Control1_ST.zw;
			#ifdef _HasWeightMap1
				half4 staticSwitch12_g19 = tex2D( _Control1, uv_Control1 );
			#else
				half4 staticSwitch12_g19 = _Vector0;
			#endif
			half4 Control125_g19 = staticSwitch12_g19;
			float2 uv_Control2 = i.uv_texcoord * _Control2_ST.xy + _Control2_ST.zw;
			#ifdef _HasWeightMap2
				half4 staticSwitch11_g19 = tex2D( _Control2, uv_Control2 );
			#else
				half4 staticSwitch11_g19 = _Vector0;
			#endif
			half4 Control225_g19 = staticSwitch11_g19;
			half4 Weights25_g19 = float4( 1,0,0,0 );
			half4 Indices25_g19 = float4( 1,0,0,0 );
			{
			Max4WeightLayer(Control025_g19, Control125_g19, Control225_g19, Weights25_g19, Indices25_g19);
			}
			half4 LayerWeights61_g19 = Weights25_g19;
			float4 Weight19_g19 = LayerWeights61_g19;
			half4 break26_g19 = Indices25_g19;
			int temp_output_7_0_g25 = (int)break26_g19.x;
			#ifdef ENABLE_LAYER_UV_SCALE
				half staticSwitch15_g25 = LayerUVScales[temp_output_7_0_g25];
			#else
				half staticSwitch15_g25 = _UVScale;
			#endif
			half2 temp_output_14_0_g25 = ( i.uv_texcoord * staticSwitch15_g25 );
			half uvInTexel10_g25 = ( temp_output_14_0_g25 * _TextureArray0_TexelSize.x ).x;
			half localMipMapLevel10_g25 = MipMapLevel10_g25( uvInTexel10_g25 );
			float4 Layer119_g19 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g25,(float)temp_output_7_0_g25), localMipMapLevel10_g25 );
			int temp_output_7_0_g22 = (int)break26_g19.y;
			#ifdef ENABLE_LAYER_UV_SCALE
				half staticSwitch15_g22 = LayerUVScales[temp_output_7_0_g22];
			#else
				half staticSwitch15_g22 = _UVScale;
			#endif
			half2 temp_output_14_0_g22 = ( i.uv_texcoord * staticSwitch15_g22 );
			half uvInTexel10_g22 = ( temp_output_14_0_g22 * _TextureArray0_TexelSize.x ).x;
			half localMipMapLevel10_g22 = MipMapLevel10_g22( uvInTexel10_g22 );
			float4 Layer219_g19 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g22,(float)temp_output_7_0_g22), localMipMapLevel10_g22 );
			int temp_output_7_0_g23 = (int)break26_g19.z;
			#ifdef ENABLE_LAYER_UV_SCALE
				half staticSwitch15_g23 = LayerUVScales[temp_output_7_0_g23];
			#else
				half staticSwitch15_g23 = _UVScale;
			#endif
			half2 temp_output_14_0_g23 = ( i.uv_texcoord * staticSwitch15_g23 );
			half uvInTexel10_g23 = ( temp_output_14_0_g23 * _TextureArray0_TexelSize.x ).x;
			half localMipMapLevel10_g23 = MipMapLevel10_g23( uvInTexel10_g23 );
			float4 Layer319_g19 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g23,(float)temp_output_7_0_g23), localMipMapLevel10_g23 );
			int temp_output_7_0_g24 = (int)break26_g19.w;
			#ifdef ENABLE_LAYER_UV_SCALE
				half staticSwitch15_g24 = LayerUVScales[temp_output_7_0_g24];
			#else
				half staticSwitch15_g24 = _UVScale;
			#endif
			half2 temp_output_14_0_g24 = ( i.uv_texcoord * staticSwitch15_g24 );
			half uvInTexel10_g24 = ( temp_output_14_0_g24 * _TextureArray0_TexelSize.x ).x;
			half localMipMapLevel10_g24 = MipMapLevel10_g24( uvInTexel10_g24 );
			float4 Layer419_g19 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g24,(float)temp_output_7_0_g24), localMipMapLevel10_g24 );
			float4 localWeightedBlend419_g19 = WeightedBlend419_g19( Weight19_g19 , Layer119_g19 , Layer219_g19 , Layer319_g19 , Layer419_g19 );
			o.Albedo = (localWeightedBlend419_g19).xyz;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Nature/Terrain/Diffuse"
	CustomEditor "MTE.MTEColorOnlyTextureArrayShaderGUI"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;216;2276.232,-502.9881;Half;False;True;-1;3;MTE.MTEColorOnlyTextureArrayShaderGUI;0;0;Lambert;MTE/Surface/TextureArray/Lambert;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;Nature/Terrain/Diffuse;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;1;Include;../MTECommonBuiltinRP.hlsl;False;fa510ed2a5a3b2d4a9ef902d0fbdd6e2;Custom;False;0;0;;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.FunctionNode;257;1929.131,-503.0638;Inherit;False;MTE_BuiltinRP_TextureArrayCore_Color;0;;19;9665015b962c47646885fa699f9af9c8;0;0;1;FLOAT3;0
WireConnection;216;0;257;0
ASEEND*/
//CHKSM=6570B15DCB47DA419AB76155F613C77B43308B99