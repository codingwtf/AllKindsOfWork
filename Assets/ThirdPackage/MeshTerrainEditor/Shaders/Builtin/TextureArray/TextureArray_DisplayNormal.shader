// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MTE/Standard/TextureArray_DisplayNormal"
{
	Properties
	{
		_Control0("Control0", 2D) = "white" {}
		_TextureArray0("TextureArray0", 2DArray) = "white" {}
		[Toggle(ENABLE_LAYER_UV_SCALE)] ENABLE_LAYER_UV_SCALE("Enable per-layer UV Scale", Float) = 1
		_UVScale("UV Scale", Range( 0.01 , 100)) = 1
		_Control1("Control1", 2D) = "white" {}
		_Control2("Control2", 2D) = "white" {}
		_NormalIntensity("Normal Intensity", Range( 0.01 , 10)) = 1
		[Toggle(ENABLE_NORMAL_INTENSITY)] ENABLE_NORMAL_INTENSITY("Enable Normal Intensity", Float) = 1
		[Toggle(ENABLE_LAYER_UV_SCALE)] ENABLE_LAYER_UV_SCALE1("Enable per-layer UV Scale", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
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

		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows 
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


		inline float MipMapLevel42_g65( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel42_g62( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel42_g63( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		inline float MipMapLevel42_g64( float uvInTexel )
		{
			return MipMapLevel(uvInTexel);
		}


		float4 WeightedBlend417_g26( half4 Weight, float4 Layer1, float4 Layer2, float4 Layer3, float4 Layer4 )
		{
			return Layer1 * Weight.r + Layer2 * Weight.g + Layer3 * Weight.b + Layer4 * Weight.a;
		}


		inline float3 RestoreNormal91_g26( float2 NormalXY, float Intensity )
		{
			return RestoreNormal(NormalXY, Intensity);
		}


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			int localGetMax4WeightLayers25_g26 = ( 0 );
			float2 uv_Control0 = i.uv_texcoord * _Control0_ST.xy + _Control0_ST.zw;
			float4 Control025_g26 = tex2D( _Control0, uv_Control0 );
			float4 _Vector0 = float4(0,0,0,0);
			float2 uv_Control1 = i.uv_texcoord * _Control1_ST.xy + _Control1_ST.zw;
			#ifdef _HasWeightMap1
				float4 staticSwitch12_g26 = tex2D( _Control1, uv_Control1 );
			#else
				float4 staticSwitch12_g26 = _Vector0;
			#endif
			float4 Control125_g26 = staticSwitch12_g26;
			float2 uv_Control2 = i.uv_texcoord * _Control2_ST.xy + _Control2_ST.zw;
			#ifdef _HasWeightMap2
				float4 staticSwitch11_g26 = tex2D( _Control2, uv_Control2 );
			#else
				float4 staticSwitch11_g26 = _Vector0;
			#endif
			float4 Control225_g26 = staticSwitch11_g26;
			float4 Weights25_g26 = float4( 1,0,0,0 );
			float4 Indices25_g26 = float4( 1,0,0,0 );
			{
			Max4WeightLayer(Control025_g26, Control125_g26, Control225_g26, Weights25_g26, Indices25_g26);
			}
			float4 LayerWeights61_g26 = Weights25_g26;
			float4 Weight17_g26 = LayerWeights61_g26;
			float4 break26_g26 = Indices25_g26;
			int temp_output_38_0_g65 = (int)break26_g26.x;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g65 = LayerUVScales[temp_output_38_0_g65];
			#else
				float staticSwitch40_g65 = _UVScale;
			#endif
			float2 temp_output_34_0_g65 = ( i.uv_texcoord * staticSwitch40_g65 );
			float uvInTexel42_g65 = ( temp_output_34_0_g65 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel42_g65 = MipMapLevel42_g65( uvInTexel42_g65 );
			float4 Layer117_g26 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_34_0_g65,(float)temp_output_38_0_g65), localMipMapLevel42_g65 );
			int temp_output_38_0_g62 = (int)break26_g26.y;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g62 = LayerUVScales[temp_output_38_0_g62];
			#else
				float staticSwitch40_g62 = _UVScale;
			#endif
			float2 temp_output_34_0_g62 = ( i.uv_texcoord * staticSwitch40_g62 );
			float uvInTexel42_g62 = ( temp_output_34_0_g62 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel42_g62 = MipMapLevel42_g62( uvInTexel42_g62 );
			float4 Layer217_g26 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_34_0_g62,(float)temp_output_38_0_g62), localMipMapLevel42_g62 );
			int temp_output_38_0_g63 = (int)break26_g26.z;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g63 = LayerUVScales[temp_output_38_0_g63];
			#else
				float staticSwitch40_g63 = _UVScale;
			#endif
			float2 temp_output_34_0_g63 = ( i.uv_texcoord * staticSwitch40_g63 );
			float uvInTexel42_g63 = ( temp_output_34_0_g63 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel42_g63 = MipMapLevel42_g63( uvInTexel42_g63 );
			float4 Layer317_g26 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_34_0_g63,(float)temp_output_38_0_g63), localMipMapLevel42_g63 );
			int temp_output_38_0_g64 = (int)break26_g26.w;
			#ifdef ENABLE_LAYER_UV_SCALE
				float staticSwitch40_g64 = LayerUVScales[temp_output_38_0_g64];
			#else
				float staticSwitch40_g64 = _UVScale;
			#endif
			float2 temp_output_34_0_g64 = ( i.uv_texcoord * staticSwitch40_g64 );
			float uvInTexel42_g64 = ( temp_output_34_0_g64 * _TextureArray0_TexelSize.x ).x;
			float localMipMapLevel42_g64 = MipMapLevel42_g64( uvInTexel42_g64 );
			float4 Layer417_g26 = SAMPLE_TEXTURE2D_ARRAY_LOD( _TextureArray0, sampler_TextureArray0, float3(temp_output_34_0_g64,(float)temp_output_38_0_g64), localMipMapLevel42_g64 );
			float4 localWeightedBlend417_g26 = WeightedBlend417_g26( Weight17_g26 , Layer117_g26 , Layer217_g26 , Layer317_g26 , Layer417_g26 );
			float2 NormalXY91_g26 = (localWeightedBlend417_g26).yz;
			#ifdef ENABLE_NORMAL_INTENSITY
				float staticSwitch63_g26 = _NormalIntensity;
			#else
				float staticSwitch63_g26 = 1.0;
			#endif
			float Intensity91_g26 = staticSwitch63_g26;
			float3 localRestoreNormal91_g26 = RestoreNormal91_g26( NormalXY91_g26 , Intensity91_g26 );
			o.Emission = (localRestoreNormal91_g26*0.5 + 0.5);
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "MTE.MTETextureArrayShaderGUI"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.CommentaryNode;247;2496.368,-955.5007;Inherit;False;306;183;Conver normal in [-1,1] to RGB in [0, 1];1;248;;1,1,1,1;0;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;31;2822.106,-951.5565;Float;False;True;-1;2;MTE.MTETextureArrayShaderGUI;0;0;Unlit;MTE/Standard/TextureArray_DisplayNormal;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;1;Include;../MTECommonBuiltinRP.hlsl;False;fa510ed2a5a3b2d4a9ef902d0fbdd6e2;Custom;False;0;0;;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;248;2561.368,-905.5007;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;258;2179.694,-929.568;Inherit;False;MTE_BuiltinRP_TextureArrayCore;0;;26;00dbb1b07fae2004c86d7610115900b1;0;0;5;FLOAT3;0;FLOAT3;93;FLOAT;94;FLOAT;92;FLOAT;95
WireConnection;31;2;248;0
WireConnection;248;0;258;93
ASEEND*/
//CHKSM=BEA6ED114024022CFCCF363597A9BDC046B07C9C