#ifndef OTP_BRUSH_INCLUDED
#define OTP_BRUSH_INCLUDED

#include "UnityCG.cginc"

//#define JITTER

float3 _PrevPos;
float3 _CurPos;
float3 _PrevNorm;
float3 _CurNorm;
float _PrevIntensity;
float _CurIntensity;
float _BrushRadius;
float _BrushStrength;
float4 _BrushMaskTexelSize;

struct appdata_brush
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 texcoord : TEXCOORD0;
};

struct v2g_brush
{
    float2 texcoord : TEXCOORD0;
    float3 worldPos : TEXCOORD1;
    float3 worldNormal : TEXCOORD2;
};

struct g2f_brush
{
    float4 pos : SV_POSITION;
    float3 worldPos : TEXCOORD0;
    float3 worldNormal : NORMAL;
};

v2g_brush vert_brush(appdata_brush v)
{
    v2g_brush o;
    o.texcoord = v.texcoord;
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
    o.worldNormal = UnityObjectToWorldNormal(v.normal);
    return o;
}

[maxvertexcount(12)]
void geo_brush(triangle v2g_brush input[3], inout TriangleStream<g2f_brush> triStream)
{
    float2 jitter[4] =
    {
        float2(-1, 0),
        float2( 1, 0),
        float2( 0,-1),
        float2( 0, 1)
    };
    #ifdef JITTER
    for(int pi = 0; pi < 4; ++pi) // 4 Pass
    #else
    int pi = 0;
    #endif
    {
        for(int vi = 0; vi < 3; ++vi)
        {
            g2f_brush o;
            #ifdef JITTER
            float2 texcoord = input[vi].texcoord + jitter[pi] * _BrushMaskTexelSize.xy;
            #else
            float2 texcoord = input[vi].texcoord;
            #endif
            o.pos = float4(texcoord * 2 - 1, 0, 1);
        #if UNITY_UV_STARTS_AT_TOP
            o.pos.y = -o.pos.y;
        #endif
            o.worldPos = input[vi].worldPos;
            o.worldNormal = input[vi].worldNormal;
            triStream.Append(o);
        }
        triStream.RestartStrip();
    }
}

#endif