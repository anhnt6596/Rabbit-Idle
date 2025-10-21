Shader "Unlit/DarknessMask"
{
    Properties {
        _LightTex ("Light Texture", 2D) = "white" {}
        _DarkColor ("Dark Color", Color) = (0.0, 0.0, 0.1, 1)
    }
    SubShader {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Blend DstColor Zero
        ZWrite Off
        Cull Off
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _LightTex;
            fixed4 _DarkColor;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // ??c brightness t? LightMap
                float brightness = tex2D(_LightTex, i.uv).r;
                // blend gi?a t?i và sáng
                return lerp(_DarkColor, fixed4(1,1,1,1), brightness);
            }
            ENDCG
        }
    }
}