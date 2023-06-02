Shader "Hidden/PPLightning"
{
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off
        Blend One One
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            uniform fixed4 _LightningColor;

            fixed4 frag (v2f i) : SV_Target
            {
                return _LightningColor;
            }
            ENDCG
        }
    }
}
