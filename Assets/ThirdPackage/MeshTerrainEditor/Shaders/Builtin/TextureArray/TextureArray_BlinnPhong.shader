// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MTE/Surface/TextureArray/BlinnPhong"
{
	Properties
	{
		_SpecColor("Specular Color",Color)=(1,1,1,1)
		_Control0("Control0", 2D) = "white" {}
		_TextureArray1("TextureArray1", 2DArray) = "white" {}
		_TextureArray0("TextureArray0", 2DArray) = "white" {}
		[Toggle(ENABLE_LAYER_UV_SCALE)] ENABLE_LAYER_UV_SCALE("Enable per-layer UV Scale", Float) = 1
		_UVScale("UV Scale", Range( 0.01 , 100)) = 1
		_Control1("Control1", 2D) = "white" {}
		_Control2("Control2", 2D) = "white" {}
		_NormalIntensity("Normal Intensity", Range( 0.01 , 10)) = 1
		[Toggle(ENABLE_NORMAL_INTENSITY)] ENABLE_NORMAL_INTENSITY("Enable Normal Intensity", Float) = 1
		_Shininess("Shininess", Range( 0.01 , 1)) = 1
		_Gloss("Gloss", Range( 0 , 1)) = 0.75
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

		#pragma surface surf BlinnPhong keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(_TextureArray0);
		uniform float _UVScale;
		UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(_TextureArray1);
		uniform float LayerUVScales[12];
		uniform sampler2D _Control0;
		uniform float4 _Control0_ST;
		uniform sampler2D _Control1;
		uniform float4 _Control1_ST;
		uniform sampler2D _Control2;
		uniform float4 _Control2_ST;
		float4 _TextureArray1_TexelSize;
		SamplerState sampler_TextureArray1;
		uniform float _NormalIntensity;
		float4 _TextureArray0_TexelSize;
		SamplerState sampler_TextureArray0;
		uniform float _Shininess;
		uniform float _Gloss;


		inline float MipMapLevel42_g111( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel42_g108( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel42_g109( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel42_g110( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		float4 WeightedBlend417_g103( half4 Weight, float4 Layer1, float4 Layer2, float4 Layer3, float4 Layer4 )
		{
			return Layer1 * Weight.r + Layer2 * Weight.g + Layer3 * Weight.b + Layer4 * Weight.a;
		}


		inline float MipMapLevel10_g104( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel10_g105( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel10_g106( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel10_g107( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		float4 WeightedBlend419_g103( half4 Weight, float4 Layer1, float4 Layer2, float4 Layer3, float4 Layer4 )
		{
			return Layer1 * Weight.r + Layer2 * Weight.g + Layer3 * Weight.b + Layer4 * Weight.a;
		}


		void surf( Input i , inout SurfaceOutput o )
		{
			float4 localGetMax4WeightLayers25_g103 = ( float4( 0,0,0,0 ) );
			float2 uv_Control0 = i.uv_texcoord * _Control0_ST.xy + _Control0_ST.zw;
			float4 Control025_g103 = tex2D( _Control0, uv_Control0 );
			float4 _Vector0 = float4(0,0,0,0);
			float2 uv_Control1 = i.uv_texcoord * _Control1_ST.xy + _Control1_ST.zw;
			#ifdef _HasWeightMap1
				float4 staticSwitch12_g103 = tex2D( _Control1, uv_Control1 );
			#else
				float4 staticSwitch12_g103 = _Vector0;
			#endif
			float4 Control125_g103 = staticSwitch12_g103;
			float2 uv_Control2 = i.uv_texcoord * _Control2_ST.xy + _Control2_ST.zw;
			#ifdef _HasWeightMap2
				float4 staticSwitch11_g103 = tex2D( _Control2, uv_Control2 );
			#else
				float4 staticSwitch11_g103 = _Vector0;
			#endif
			float4 Control225_g103 = staticSwitch11_g103;
			float4 Weights25_g103 = float4( 1,0,0,0 );
			float4 Indices25_g103 = float4( 1,0,0,0 );
			{
			Max4WeightLayer(Control025_g103, Control125_g103, Control225_g103, Weights25_g103, Indices25_g103);
			}
			float4 LayerWeights61_g103 = Weights25_g103;
			float4 Weight17_g103 = LayerWeights61_g103;
			float4 break26_g103 = Indices25_g103;
			int temp_output_38_0_g111 = (int)break26_g103.x;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g111 = LayerUVScales[temp_output_38_0_g111];
			#else
				float staticSwitch40_g111 = _UVScale;
			#endif
			float2 temp_output_34_0_g111 = ( i.uv_texcoord * staticSwitch40_g111 );
			float uvInTexel42_g111 = ( temp_output_34_0_g111 * _TextureArray1_TexelSize.x ).x;
			float localMipMapLevel42_g111 = MipMapLevel42_g111( uvInTexel42_g111 );
			float4 Layer117_g103 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray1, sampler_TextureArray1, float3(temp_output_34_0_g111,(float)temp_output_38_0_g111), localMipMapLevel42_g111 );
			int temp_output_38_0_g108 = (int)break26_g103.y;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g108 = LayerUVScales[temp_output_38_0_g108];
			#else
				float staticSwitch40_g108 = _UVScale;
			#endif
			float2 temp_output_34_0_g108 = ( i.uv_texcoord * staticSwitch40_g108 );
			float uvInTexel42_g108 = ( temp_output_34_0_g108 * _TextureArray1_TexelSize.x ).x;
			float localMipMapLevel42_g108 = MipMapLevel42_g108( uvInTexel42_g108 );
			float4 Layer217_g103 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray1, sampler_TextureArray1, float3(temp_output_34_0_g108,(float)temp_output_38_0_g108), localMipMapLevel42_g108 );
			int temp_output_38_0_g109 = (int)break26_g103.z;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g109 = LayerUVScales[temp_output_38_0_g109];
			#else
				float staticSwitch40_g109 = _UVScale;
			#endif
			float2 temp_output_34_0_g109 = ( i.uv_texcoord * staticSwitch40_g109 );
			float uvInTexel42_g109 = ( temp_output_34_0_g109 * _TextureArray1_TexelSize.x ).x;
			float localMipMapLevel42_g109 = MipMapLevel42_g109( uvInTexel42_g109 );
			float4 Layer317_g103 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray1, sampler_TextureArray1, float3(temp_output_34_0_g109,(float)temp_output_38_0_g109), localMipMapLevel42_g109 );
			int temp_output_38_0_g110 = (int)break26_g103.w;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g110 = LayerUVScales[temp_output_38_0_g110];
			#else
				float staticSwitch40_g110 = _UVScale;
			#endif
			float2 temp_output_34_0_g110 = ( i.uv_texcoord * staticSwitch40_g110 );
			float uvInTexel42_g110 = ( temp_output_34_0_g110 * _TextureArray1_TexelSize.x ).x;
			float localMipMapLevel42_g110 = MipMapLevel42_g110( uvInTexel42_g110 );
			float4 Layer417_g103 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray1, sampler_TextureArray1, float3(temp_output_34_0_g110,(float)temp_output_38_0_g110), localMipMapLevel42_g110 );
			float4 localWeightedBlend417_g103 = WeightedBlend417_g103( Weight17_g103 , Layer117_g103 , Layer217_g103 , Layer317_g103 , Layer417_g103 );
			#ifdef ENABLE_NORMAL_INTENSITY
				float staticSwitch106_g103 = _NormalIntensity;
			#else
				float staticSwitch106_g103 = 1.0;
			#endif
			o.Normal = UnpackScaleNormal( localWeightedBlend417_g103, staticSwitch106_g103 );
			float4 Weight19_g103 = LayerWeights61_g103;
			int temp_output_7_0_g104 = (int)break26_g103.x;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g104 = LayerUVScales[temp_output_7_0_g104];
			#else
				float staticSwitch15_g104 = _UVScale;
			#endif
			float2 temp_output_14_0_g104 = ( i.uv_texcoord * staticSwitch15_g104 );
			float uvInTexel10_g104 = ( temp_output_14_0_g104 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g104 = MipMapLevel10_g104( uvInTexel10_g104 );
			float4 Layer119_g103 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g104,(float)temp_output_7_0_g104), localMipMapLevel10_g104 );
			int temp_output_7_0_g105 = (int)break26_g103.y;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g105 = LayerUVScales[temp_output_7_0_g105];
			#else
				float staticSwitch15_g105 = _UVScale;
			#endif
			float2 temp_output_14_0_g105 = ( i.uv_texcoord * staticSwitch15_g105 );
			float uvInTexel10_g105 = ( temp_output_14_0_g105 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g105 = MipMapLevel10_g105( uvInTexel10_g105 );
			float4 Layer219_g103 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g105,(float)temp_output_7_0_g105), localMipMapLevel10_g105 );
			int temp_output_7_0_g106 = (int)break26_g103.z;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g106 = LayerUVScales[temp_output_7_0_g106];
			#else
				float staticSwitch15_g106 = _UVScale;
			#endif
			float2 temp_output_14_0_g106 = ( i.uv_texcoord * staticSwitch15_g106 );
			float uvInTexel10_g106 = ( temp_output_14_0_g106 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g106 = MipMapLevel10_g106( uvInTexel10_g106 );
			float4 Layer319_g103 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g106,(float)temp_output_7_0_g106), localMipMapLevel10_g106 );
			int temp_output_7_0_g107 = (int)break26_g103.w;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g107 = LayerUVScales[temp_output_7_0_g107];
			#else
				float staticSwitch15_g107 = _UVScale;
			#endif
			float2 temp_output_14_0_g107 = ( i.uv_texcoord * staticSwitch15_g107 );
			float uvInTexel10_g107 = ( temp_output_14_0_g107 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g107 = MipMapLevel10_g107( uvInTexel10_g107 );
			float4 Layer419_g103 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g107,(float)temp_output_7_0_g107), localMipMapLevel10_g107 );
			float4 localWeightedBlend419_g103 = WeightedBlend419_g103( Weight19_g103 , Layer119_g103 , Layer219_g103 , Layer319_g103 , Layer419_g103 );
			o.Albedo = (localWeightedBlend419_g103).xyz;
			o.Specular = _Shininess;
			o.Gloss = _Gloss;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Hidden/InternalErrorShader"
	CustomEditor "MTE.MTEColorAndNormalTextureArrayShaderGUI"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.CommentaryNode;283;2150.791,-251.5399;Inherit;False;350;165;specular intensity;1;275;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;277;2179.872,-414.9338;Inherit;False;298.9191;132.3525;specular power;1;276;;1,1,1,1;0;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;216;2619.232,-508.9881;Float;False;True;-1;2;MTE.MTEColorAndNormalTextureArrayShaderGUI;0;0;BlinnPhong;MTE/Surface/TextureArray/BlinnPhong;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;Hidden/InternalErrorShader;-1;-1;-1;-1;0;False;0;0;False;;0;0;False;;1;Include;../MTECommonBuiltinRP.hlsl;False;fa510ed2a5a3b2d4a9ef902d0fbdd6e2;Custom;False;0;0;;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.RangedFloatNode;275;2200.791,-201.5399;Inherit;False;Property;_Gloss;Gloss;42;0;Create;True;0;0;0;False;0;False;0.75;0.385;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;276;2204.637,-368.9338;Inherit;False;Property;_Shininess;Shininess;41;0;Create;True;0;0;0;False;0;False;1;0.612;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;290;2112.978,-509.6666;Inherit;False;MTE_BuiltinRP_TextureArrayCore_ColorAndNormal;1;;103;fbea9b15efa251f42a4d93433849dcae;0;0;2;FLOAT3;0;FLOAT3;93
WireConnection;216;0;290;0
WireConnection;216;1;290;93
WireConnection;216;3;276;0
WireConnection;216;4;275;0
ASEEND*/
//CHKSM=5DC1727CC0538458EE825BDACFC12AB88DF892EF