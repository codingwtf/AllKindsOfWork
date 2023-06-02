Shader "Hidden/OTP/Brush/RampPreview"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            sampler2D _MainTex;

            v2f_img vert (appdata_img v)
            {
                v2f_img o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag (v2f_img i) : SV_Target
            {
                float u = 1 - distance(i.uv, 0.5) * 2;
                return tex2D(_MainTex, float2(u, 0.5)) * step(0, u);
            }
            ENDCG
        }
    }
}
