Shader "Hidden/OTP/Painter/TerrainSplat"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma multi_compile _SPLAT_X2 _SPLAT_X3 
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define round(x) floor((x) + 0.5)
            #define UnpackIndex(idx) floor((idx) * 128 + 0.5)
            #define PackIndex(idx) ((idx) / 128.0)
            #define PackWeight(weight) ((weight) / dot((weight), 1))

            sampler2D _BrushMask;
            int _Index;
            sampler2D _Src0;
            sampler2D _Src1;
            #define _SrcIndex _Src0
            #define _SrcWeight _Src1

            v2f_img vert (appdata_img v)
            {
                v2f_img o;
                v.vertex.xy = v.vertex.xy * 2 - 1;
            #if UNITY_UV_STARTS_AT_TOP
                v.vertex.y = - v.vertex.y;
            #endif
                o.pos = v.vertex;
                o.uv = v.texcoord;
                return o;
            }

            void frag (v2f_img i, 
                out fixed4 outIndex  : SV_Target0, 
                out fixed4 outWeight : SV_Target1)
            {
                outIndex = 0;
                outWeight = 0;

                fixed brushMask = tex2D(_BrushMask, i.uv).r;
                fixed mask = step(1e-8, brushMask);

            #if _SPLAT_X2
                int2   srcIndex = UnpackIndex(tex2D(_SrcIndex, i.uv).rg);
                fixed2 srcWeight = tex2D(_SrcWeight, i.uv).rg;
                float3 weight = float3(srcWeight, 0);
                int2 index = srcIndex;
                if(_Index == srcIndex.r)
                {
                    weight.r = lerp(srcWeight.r, 1, brushMask);
                }
                else if(_Index == srcIndex.g)
                {
                    weight.g = lerp(srcWeight.g, 1, brushMask);
                    if(weight.g > srcWeight.r)
                    {
                        weight.r = weight.g;
                        weight.g = srcWeight.r;

                        index.r = srcIndex.g;
                        index.g = srcIndex.r;
                    }
                }
                else
                {
                    weight.b = lerp(weight.b, 1, brushMask);
                    float screenMask = (1 - (1 - srcWeight.g) * (1 - weight.b));
                    if(weight.b > srcWeight.r)
                    {
                        weight.r = weight.b;
                        index.r = _Index;

                        weight.g = srcWeight.r;
                        index.g = srcIndex.r;
                    }
                    else if(weight.b > pow(srcWeight.g, 4.0f))
                    {
                        weight.g = screenMask;
                        index.g = _Index;
                    }
                }

                outIndex.rg  = PackIndex(lerp(srcIndex, index, mask));
                outWeight.rg = PackWeight(lerp(srcWeight, weight, mask));
            #else
                int3   srcIndex  = UnpackIndex(tex2D(_SrcIndex, i.uv).rgb);
                fixed3 srcWeight = tex2D(_SrcWeight, i.uv).rgb;
                float4 weight = float4(srcWeight, 0);
                int3 index = srcIndex;
                
                if (_Index == srcIndex.r)
                {
                    weight.r = lerp(weight.r, 1, brushMask);
                }
                else if(_Index == srcIndex.g)
                {
                    weight.g = lerp(weight.g, 1, brushMask);
                    if(weight.g > srcWeight.r)
                    {
                        weight.r = weight.g;
                        weight.g = srcWeight.r;

                        index.r = srcIndex.g;
                        index.g = srcIndex.r;
                    }
                }
                else if(_Index == srcIndex.b)
                {
                    weight.b = lerp(weight.g, 1, brushMask);
                    if(weight.b > srcWeight.r)
                    {
                        weight.r = weight.b;
                        weight.b = srcWeight.r;
                        index.r = srcIndex.b;
                        index.b = srcIndex.r;
                    }
                    else if(weight.b > srcWeight.g)
                    {
                        weight.g = weight.b;
                        weight.b = srcWeight.g;
                        index.g = srcIndex.b;
                        index.b = srcIndex.g;
                    }
                }
                else
                {
                    weight.a = brushMask;
                    if(weight.a > srcWeight.r)
                    {
                        weight.r = weight.a;
                        weight.g = srcWeight.r;
                        weight.b = srcWeight.g;
                        index.r = _Index;
                        index.g = srcIndex.r;
                        index.b = srcIndex.g;
                    }
                    else if(weight.a > srcWeight.g)
                    {
                        weight.g = weight.a;
                        weight.b = srcWeight.g;
                        index.g = _Index;
                        index.b = srcIndex.g;
                    }
                    else if(weight.a > srcWeight.b)
                    {
                        weight.b = weight.a;
                        index.b = _Index;
                    }
                }

                outIndex.rgb  = PackIndex(lerp(srcIndex, index, mask));
                outWeight.rgb = PackWeight(lerp(srcWeight, weight, mask));
            #endif
            }
            ENDCG
        }
    }
}