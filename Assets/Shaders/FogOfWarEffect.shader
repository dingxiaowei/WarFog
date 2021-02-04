Shader "FOW/FogOfWarEffect"
{
    Properties
    {
        //		_CameraColorTex ("CameraColorTex", 2D) = "white" {}
        _FogColor("FogColor", color) = (0,0,0,1)
        //		_FogTex ("FogTex", 2D) = "black" {}
        _MixValue("MixValue", float) = 0
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv_depth : TEXCOORD1;
                float3 interpolatedRay : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _CameraColorTexture;
            float4 _CameraColorTexture_TexelSize;
            sampler2D _CameraDepthTexture;
            float4x4 _FrustumCornersRay;
            sampler2D _FogTex;

            half _MixValue;

            half4 _FogColor;

            float4x4 internal_WorldToProjector;

            v2f vert(appdata_img v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.uv_depth = v.texcoord;

                #if UNITY_UV_STARTS_AT_TOP
                if (_CameraColorTexture_TexelSize.y < 0)
                    o.uv_depth.y = 1 - o.uv_depth.y;
                #endif

                //根据纹理坐标特性判定属于哪个角
                int index = 0;
                if (v.texcoord.x < 0.5 && v.texcoord.y < 0.5)
                {
                    index = 0;
                }
                else if (v.texcoord.x > 0.5 && v.texcoord.y < 0.5)
                {
                    index = 1;
                }
                else if (v.texcoord.x > 0.5 && v.texcoord.y > 0.5)
                {
                    index = 2;
                }
                else
                {
                    index = 3;
                }

                #if UNITY_UV_STARTS_AT_TOP
                if (_CameraColorTexture_TexelSize.y < 0)
                    index = 3 - index;
                #endif

                o.interpolatedRay = _FrustumCornersRay[index].xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_CameraColorTexture, UnityStereoTransformScreenSpaceTex(i.uv));

                // 先对深度纹理进行采样，再得到视角空间下的线性深度值
                float linearDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv_depth));
                // 得到世界空间下的位置
                float3 worldPos = _WorldSpaceCameraPos + linearDepth * i.interpolatedRay.xyz;
                // 通过internal_CameraToProjector矩阵最终得到战争迷雾uv空间坐标
                worldPos = mul(internal_WorldToProjector, worldPos);
                // 使用战争迷雾uv空间坐标对迷雾纹理进行采样
                fixed3 tex = tex2D(_FogTex, worldPos.xy).rgb;

                float2 atten = saturate((0.5 - abs(worldPos.xy - 0.5)) / (1 - 0.9));

                fixed3 col;
                col.rgb = lerp(_FogColor.rgb, fixed3(1, 1, 1), tex.r * _FogColor.a);

                fixed visual = lerp(tex.b, tex.g, _MixValue);
                col.rgb = lerp(col.rgb, fixed3(1, 1, 1), visual) * atten.x * atten.y;

                c.rgb *= col.rgb;
                c.a = 1;
                return c;
            }
            ENDCG
        }
    }
}