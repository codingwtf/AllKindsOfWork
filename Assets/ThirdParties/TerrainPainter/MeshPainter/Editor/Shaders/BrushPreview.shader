Shader "Hidden/OTP/BrushPreview"
{
    Properties
    {
        _BrushMask ("Brush Msak", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            sampler2D _BrushMask;

            #include "UnityCG.cginc"

            fixed4 frag (v2f_img i) : SV_Target
            {
                fixed brush = tex2D(_BrushMask, i.uv).r;
                return fixed4(brush, brush, brush, brush);
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            sampler2D _BrushMask;

            #include "UnityCG.cginc"

            fixed4 frag (v2f_img i) : SV_Target
            {
                fixed brush = tex2D(_BrushMask, i.uv).r;
                float2 dxdy = abs(float2(ddx(brush), ddy(brush)));
                fixed edge = step(brush, 0) * (1 - step(max(dxdy.x, dxdy.y), 0));
                return edge;
            }
            ENDCG
        }
    }
}
