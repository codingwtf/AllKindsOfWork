Shader "Hidden/OTP/Brush/RampedSDF"
{
    Properties
    {
        _RampMap ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Conservative True

        Stencil
        {
            Ref 1
            Comp NotEqual
            Pass Replace
            Fail Keep
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_brush
            #pragma geometry geo_brush
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../CGIncludes/Brush.cginc"

            sampler2D _RampMap;

            float DistLine(float3 a, float3 b, float3 p)
			{
				float3 ab = b - a;
				float t = saturate(dot(p - a, ab) / dot(ab, ab));
				float3 c = a + t * ab;
				return length(p - c);
			}

            fixed4 frag (g2f_brush i) : SV_Target
            {
                float dist = 1 - saturate(DistLine(_PrevPos, _CurPos, i.worldPos) / _BrushRadius);
                fixed ramp = tex2D(_RampMap, float2(dist, 0.5)).r;
                float mask = step(1e-8, dist) * step(dist, 1);
                return ramp * _BrushStrength * mask;
            }
            ENDCG
        }
    }
}
