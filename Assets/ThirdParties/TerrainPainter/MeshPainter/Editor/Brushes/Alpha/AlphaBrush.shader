Shader "Hidden/OTP/Brush/Alpha"
{
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

            sampler2D _Alpha;
            float _BrushRotation;
            fixed4 _ChannelMask;
            float3 _ProjectionUp;
            float3 _ProjectionRight;

            fixed4 frag (g2f_brush i) : SV_Target
            {
                float cosTheta = cos(_BrushRotation);
                float sinTheta = sin(_BrushRotation);
                float2x2 rot = float2x2(cosTheta, -sinTheta, sinTheta, cosTheta);
                float3 projectionRight = normalize(_ProjectionRight);
                float3 projectionUp = normalize(_ProjectionUp);
                
                float3 relPos = i.worldPos - _CurPos;
                float2 uv = float2(dot(relPos, projectionRight), dot(relPos, projectionUp)).xy / _BrushRadius * 0.5 * sqrt(2);
                uv = mul(rot, uv) + 0.5;
                #ifndef UNITY_UV_STARTS_AT_TOP
                uv.y = 1 - uv.y;
                #endif
                uv = saturate(uv);
                float alpha = dot(tex2D(_Alpha, uv), _ChannelMask) * max(0, dot(normalize(i.worldNormal), _CurNorm));
                float dist = distance(_CurPos, i.worldPos);
                float3 mask = float3(step(0, uv) * step(uv, 1), step(dist, _BrushRadius));
                return alpha * _BrushStrength * mask.x * mask.y * mask.z;
            }
            ENDCG
        }
    }
}