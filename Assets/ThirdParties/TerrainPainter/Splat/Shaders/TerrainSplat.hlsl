#ifndef TERRAIN_SPLAT_INCLUDED
#define TERRAIN_SPLAT_INCLUDED

//TEXTURE2D 申明纹理 SAMPLER 后面是定义采样，后面只是名字没有特别的意义
TEXTURE2D(_IndexTex);  SAMPLER(sampler_point_clamp);
TEXTURE2D(_WeightTex); SAMPLER(sampler_linear_clamp);

uniform float _TerrainSplatTiling[16];

#define _ATLAS_ROW 4
#define _ATLAS_COL 4

#define DecodeIndex(idx) floor((idx) * 128 + 0.5)

#define MIPMAP_COUNT(tex) (log2( max(tex##_TexelSize.z, tex##_TexelSize.w) ))

// Input
CBUFFER_START(UnityPerMaterial)
#ifdef _USEATLAS
	float4 _MainTexAtlas_TexelSize;
#else
	float4 _MainTexArray_TexelSize;
#endif
CBUFFER_END

inline float MIPMAP_LEVEL(float2 dx, float2 dy)
{
	float max_dxdy_sqr = max(dot(dx, dx), dot(dy, dy));
	return 0.5 * log2(max_dxdy_sqr); // <==> log2(sqrt(max_dxdy_sqr));
}

inline float MIPMAP_LEVEL(float2 uv)
{
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);
	return MIPMAP_LEVEL(dx, dy);
}

float3 blend_whiteout(float3 n1, float3 n2)
{
    float3 r = float3(n1.xy + n2.xy, n1.z*n2.z);
    return normalize(r);
}

float3 blend_whiteoutW(float3 n1, float3 n2, float t)
{
	float3 n = float3(
        lerp(n1.xy, n2.xy, t), 
        lerp(n1.z, 1, t) * lerp(n2.z, 1, 1 - t));
	return normalize(n);
}

float3 blend_whiteoutW(float3 n1, float3 n2, float3 n3, float w1, float w2, float w3)
{
    float3 n = float3(
        n1.xy * w1 + n2.xy * w2 + n3.xy * w3, 
        lerp(n1.z, 1, 1 - w1) * lerp(n2.z, 1, 1 - w2) * lerp(n3.z, 1, 1 - w3));
    return normalize(n);
}

float3 blend_height(float high1 ,float high2,float high3, float3 control, float weight)
{
	float3 blend = float3(high1, high2, high3) * control;
	half ma = max(blend.r, max(blend.g, blend.b));
	//与权重最大的通道进行对比，高度差在_Weight范围内的将会保留,_Weight不可以为0
	blend = max(blend - ma + weight , 0) * control;
	return blend/(blend.r + blend.g + blend.b);
}

float2 blend_height(float high1 ,float high2, float2 control, float weight)
{
	float2 blend = float2(high1, high2) * control;
	half ma = max(blend.r, blend.g);
	//与权重最大的通道进行对比，高度差在_Weight范围内的将会保留,_Weight不可以为0
	blend = max(blend - ma + weight , 0) * control;
	return blend/(blend.r + blend.g);
}

#ifdef _USEATLAS
    // 使用图集模型
	float4 GetUVByIndex(float3 uv)
	{
		uint index = uv.z;
        uint2 colRow = uint2(index % _ATLAS_ROW, index / _ATLAS_ROW);
        float2 cellSize = 1 / float2(_ATLAS_COL, _ATLAS_ROW);
		float4 dxdy = float4(ddx(uv.xy), ddy(uv.xy));
        float2 packedUV = uv.xy * _TerrainSplatTiling[index];
		dxdy *= _TerrainSplatTiling[uv.z] * cellSize.xyxy;
		float mipCount = MIPMAP_COUNT(_MainTexAtlas);
        float lod = mipCount + MIPMAP_LEVEL(dxdy.xy, dxdy.zw);
        packedUV = frac(packedUV) * cellSize;
        float2 offset = _MainTexAtlas_TexelSize * pow(2.0, lod) * 0.5;
        packedUV = clamp(packedUV, offset, cellSize - offset);
        packedUV += colRow * cellSize;
        return float4(packedUV, 0, lod);
	}
	#define DECLARE_TEX_SET(tex) TEXTURE2D(tex##Atlas); SAMPLER(sampler##tex##Atlas); float4 tex##_TexelSize
	#define SAMPLE_TEX_SET(tex,uv) SAMPLE_TEXTURE2D_LOD(tex##Atlas, sampler##tex##Atlas, uv.xy, uv.w)
#else
	float4 GetUVByIndex(float3 uv)
	{
    		float4 dxdy = float4(ddx(uv.xy), ddy(uv.xy));
    		dxdy *= _TerrainSplatTiling[uv.z];
    		float lod = MIPMAP_LEVEL(dxdy.xy, dxdy.zw);
    		float mipCount = MIPMAP_COUNT(_MainTexArray);
    		return float4(uv.xy * _TerrainSplatTiling[uv.z], uv.z, mipCount + lod);
    }
	#define DECLARE_TEX_SET(tex) TEXTURE2D_ARRAY(tex##Array); SAMPLER(sampler##tex##Array)

    // 这边传入了LOD
	#define SAMPLE_TEX_SET(tex,uv) SAMPLE_TEXTURE2D_ARRAY_LOD(tex##Array, sampler##tex##Array, uv.xy, uv.z, uv.w)
#endif
#endif 