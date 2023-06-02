// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MTE/Standard/TextureArray"
{
	Properties
	{
		_Control0("Control0", 2D) = "white" {}
		_TextureArray1("TextureArray1", 2DArray) = "white" {}
		[Toggle(ENABLE_LAYER_UV_SCALE)] ENABLE_LAYER_UV_SCALE("Enable per-layer UV Scale", Float) = 1
		_UVScale("UV Scale", Range( 0.01 , 100)) = 1
		_TextureArray0("TextureArray0", 2DArray) = "white" {}
		_Control1("Control1", 2D) = "white" {}
		_Control2("Control2", 2D) = "white" {}
		_NormalIntensity("Normal Intensity", Range( 0.01 , 10)) = 1
		[Toggle(ENABLE_METALLIC)] ENABLE_METALLIC("Enable Metallic", Float) = 1
		[Toggle(ENABLE_NORMAL_INTENSITY)] ENABLE_NORMAL_INTENSITY("Enable Normal Intensity", Float) = 1
		[Toggle(ENABLE_LAYER_UV_SCALE)] ENABLE_LAYER_UV_SCALE("ENABLE_LAYER_UV_SCALE", Float) = 1
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
		#pragma shader_feature ENABLE_NORMAL_INTENSITY
		#pragma shader_feature ENABLE_METALLIC
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

		UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(_TextureArray1);
		UNITY_DECLARE_TEX2DARRAY_NOSAMPLER(_TextureArray0);
		uniform float _UVScale;
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


		inline float MipMapLevel42_g86( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel42_g83( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel42_g84( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel42_g85( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		float4 WeightedBlend417_g82( half4 Weight, float4 Layer1, float4 Layer2, float4 Layer3, float4 Layer4 )
		{
			return Layer1 * Weight.r + Layer2 * Weight.g + Layer3 * Weight.b + Layer4 * Weight.a;
		}


		inline float3 RestoreNormal91_g82( float2 NormalXY, float Intensity )
		{
			return RestoreNormal(NormalXY, Intensity);
		}


		inline float MipMapLevel10_g90( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel10_g87( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel10_g88( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel10_g89( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		float4 WeightedBlend419_g82( half4 Weight, float4 Layer1, float4 Layer2, float4 Layer3, float4 Layer4 )
		{
			return Layer1 * Weight.r + Layer2 * Weight.g + Layer3 * Weight.b + Layer4 * Weight.a;
		}


		inline float WeightedMax462_g82( float4 Weights, float a, float b, float c, float d )
		{
			return max(max(max(Weights.x * a, Weights.y * b), Weights.z * c), Weights.w * d);
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			int localGetMax4WeightLayers25_g82 = ( 0 );
			float2 uv_Control0 = i.uv_texcoord * _Control0_ST.xy + _Control0_ST.zw;
			float4 Control025_g82 = tex2D( _Control0, uv_Control0 );
			float4 _Vector0 = float4(0,0,0,0);
			float2 uv_Control1 = i.uv_texcoord * _Control1_ST.xy + _Control1_ST.zw;
			#ifdef _HasWeightMap1
				float4 staticSwitch12_g82 = tex2D( _Control1, uv_Control1 );
			#else
				float4 staticSwitch12_g82 = _Vector0;
			#endif
			float4 Control125_g82 = staticSwitch12_g82;
			float2 uv_Control2 = i.uv_texcoord * _Control2_ST.xy + _Control2_ST.zw;
			#ifdef _HasWeightMap2
				float4 staticSwitch11_g82 = tex2D( _Control2, uv_Control2 );
			#else
				float4 staticSwitch11_g82 = _Vector0;
			#endif
			float4 Control225_g82 = staticSwitch11_g82;
			float4 Weights25_g82 = float4( 1,0,0,0 );
			float4 Indices25_g82 = float4( 1,0,0,0 );
			{
			Max4WeightLayer(Control025_g82, Control125_g82, Control225_g82, Weights25_g82, Indices25_g82);
			}
			float4 LayerWeights61_g82 = Weights25_g82;
			float4 Weight17_g82 = LayerWeights61_g82;
			float4 break26_g82 = Indices25_g82;
			int temp_output_38_0_g86 = (int)break26_g82.x;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g86 = LayerUVScales[temp_output_38_0_g86];
			#else
				float staticSwitch40_g86 = _UVScale;
			#endif
			float2 temp_output_34_0_g86 = ( i.uv_texcoord * staticSwitch40_g86 );
			float uvInTexel42_g86 = ( temp_output_34_0_g86 * _TextureArray1_TexelSize.x ).x;
			float localMipMapLevel42_g86 = MipMapLevel42_g86( uvInTexel42_g86 );
			float4 Layer117_g82 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray1, sampler_TextureArray1, float3(temp_output_34_0_g86,(float)temp_output_38_0_g86), localMipMapLevel42_g86 );
			int temp_output_38_0_g83 = (int)break26_g82.y;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g83 = LayerUVScales[temp_output_38_0_g83];
			#else
				float staticSwitch40_g83 = _UVScale;
			#endif
			float2 temp_output_34_0_g83 = ( i.uv_texcoord * staticSwitch40_g83 );
			float uvInTexel42_g83 = ( temp_output_34_0_g83 * _TextureArray1_TexelSize.x ).x;
			float localMipMapLevel42_g83 = MipMapLevel42_g83( uvInTexel42_g83 );
			float4 Layer217_g82 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray1, sampler_TextureArray1, float3(temp_output_34_0_g83,(float)temp_output_38_0_g83), localMipMapLevel42_g83 );
			int temp_output_38_0_g84 = (int)break26_g82.z;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g84 = LayerUVScales[temp_output_38_0_g84];
			#else
				float staticSwitch40_g84 = _UVScale;
			#endif
			float2 temp_output_34_0_g84 = ( i.uv_texcoord * staticSwitch40_g84 );
			float uvInTexel42_g84 = ( temp_output_34_0_g84 * _TextureArray1_TexelSize.x ).x;
			float localMipMapLevel42_g84 = MipMapLevel42_g84( uvInTexel42_g84 );
			float4 Layer317_g82 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray1, sampler_TextureArray1, float3(temp_output_34_0_g84,(float)temp_output_38_0_g84), localMipMapLevel42_g84 );
			int temp_output_38_0_g85 = (int)break26_g82.w;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g85 = LayerUVScales[temp_output_38_0_g85];
			#else
				float staticSwitch40_g85 = _UVScale;
			#endif
			float2 temp_output_34_0_g85 = ( i.uv_texcoord * staticSwitch40_g85 );
			float uvInTexel42_g85 = ( temp_output_34_0_g85 * _TextureArray1_TexelSize.x ).x;
			float localMipMapLevel42_g85 = MipMapLevel42_g85( uvInTexel42_g85 );
			float4 Layer417_g82 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray1, sampler_TextureArray1, float3(temp_output_34_0_g85,(float)temp_output_38_0_g85), localMipMapLevel42_g85 );
			float4 localWeightedBlend417_g82 = WeightedBlend417_g82( Weight17_g82 , Layer117_g82 , Layer217_g82 , Layer317_g82 , Layer417_g82 );
			float2 NormalXY91_g82 = (localWeightedBlend417_g82).yz;
			#ifdef ENABLE_NORMAL_INTENSITY
				float staticSwitch63_g82 = _NormalIntensity;
			#else
				float staticSwitch63_g82 = 1.0;
			#endif
			float Intensity91_g82 = staticSwitch63_g82;
			float3 localRestoreNormal91_g82 = RestoreNormal91_g82( NormalXY91_g82 , Intensity91_g82 );
			o.Normal = localRestoreNormal91_g82;
			float4 Weight19_g82 = LayerWeights61_g82;
			int temp_output_7_0_g90 = (int)break26_g82.x;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g90 = LayerUVScales[temp_output_7_0_g90];
			#else
				float staticSwitch15_g90 = _UVScale;
			#endif
			float2 temp_output_14_0_g90 = ( i.uv_texcoord * staticSwitch15_g90 );
			float uvInTexel10_g90 = ( temp_output_14_0_g90 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g90 = MipMapLevel10_g90( uvInTexel10_g90 );
			float4 temp_output_196_0_g82 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g90,(float)temp_output_7_0_g90), localMipMapLevel10_g90 );
			float4 Layer119_g82 = temp_output_196_0_g82;
			int temp_output_7_0_g87 = (int)break26_g82.y;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g87 = LayerUVScales[temp_output_7_0_g87];
			#else
				float staticSwitch15_g87 = _UVScale;
			#endif
			float2 temp_output_14_0_g87 = ( i.uv_texcoord * staticSwitch15_g87 );
			float uvInTexel10_g87 = ( temp_output_14_0_g87 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g87 = MipMapLevel10_g87( uvInTexel10_g87 );
			float4 temp_output_193_0_g82 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g87,(float)temp_output_7_0_g87), localMipMapLevel10_g87 );
			float4 Layer219_g82 = temp_output_193_0_g82;
			int temp_output_7_0_g88 = (int)break26_g82.z;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g88 = LayerUVScales[temp_output_7_0_g88];
			#else
				float staticSwitch15_g88 = _UVScale;
			#endif
			float2 temp_output_14_0_g88 = ( i.uv_texcoord * staticSwitch15_g88 );
			float uvInTexel10_g88 = ( temp_output_14_0_g88 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g88 = MipMapLevel10_g88( uvInTexel10_g88 );
			float4 temp_output_194_0_g82 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g88,(float)temp_output_7_0_g88), localMipMapLevel10_g88 );
			float4 Layer319_g82 = temp_output_194_0_g82;
			int temp_output_7_0_g89 = (int)break26_g82.w;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch15_g89 = LayerUVScales[temp_output_7_0_g89];
			#else
				float staticSwitch15_g89 = _UVScale;
			#endif
			float2 temp_output_14_0_g89 = ( i.uv_texcoord * staticSwitch15_g89 );
			float uvInTexel10_g89 = ( temp_output_14_0_g89 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel10_g89 = MipMapLevel10_g89( uvInTexel10_g89 );
			float4 temp_output_195_0_g82 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_14_0_g89,(float)temp_output_7_0_g89), localMipMapLevel10_g89 );
			float4 Layer419_g82 = temp_output_195_0_g82;
			float4 localWeightedBlend419_g82 = WeightedBlend419_g82( Weight19_g82 , Layer119_g82 , Layer219_g82 , Layer319_g82 , Layer419_g82 );
			o.Albedo = (localWeightedBlend419_g82).xyz;
			float4 Weights62_g82 = LayerWeights61_g82;
			float a62_g82 = (temp_output_196_0_g82).a;
			float b62_g82 = (temp_output_193_0_g82).a;
			float c62_g82 = (temp_output_194_0_g82).a;
			float d62_g82 = (temp_output_195_0_g82).a;
			float localWeightedMax462_g82 = WeightedMax462_g82( Weights62_g82 , a62_g82 , b62_g82 , c62_g82 , d62_g82 );
			#ifdef ENABLE_METALLIC
				float staticSwitch65_g82 = localWeightedMax462_g82;
			#else
				float staticSwitch65_g82 = 0.0;
			#endif
			o.Metallic = staticSwitch65_g82;
			o.Smoothness = ( 1.0 - (localWeightedBlend417_g82).x );
			o.Occlusion = (localWeightedBlend417_g82).w;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;340;3010.34,-702.9327;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;MTE/Standard/TextureArray;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;1;Include;../MTECommonBuiltinRP.hlsl;False;;Custom;False;0;0;;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.FunctionNode;341;2624.121,-702.8031;Inherit;False;MTE_BuiltinRP_TextureArrayCore;0;;82;00dbb1b07fae2004c86d7610115900b1;0;0;5;FLOAT3;0;FLOAT3;93;FLOAT;94;FLOAT;92;FLOAT;95
WireConnection;340;0;341;0
WireConnection;340;1;341;93
WireConnection;340;3;341;94
WireConnection;340;4;341;92
WireConnection;340;5;341;95
ASEEND*/
//CHKSM=BE2766BACC5AC517C8A478A9E746B9261FA5592B