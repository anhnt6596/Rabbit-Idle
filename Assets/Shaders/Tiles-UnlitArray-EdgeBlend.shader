Shader "Tiles/UnlitArray_EdgeBlend"
{
    Properties
    {
        _Tiles   ("Texture2DArray", 2DArray) = "" {}
        _Tint    ("Tint", Color) = (1,1,1,1)
        _Feather ("Feather Width", Range(0.01, 0.5)) = 0.25   // độ rộng feather (theo UV)
        _Alpha   ("Max Alpha", Range(0,1)) = 1.0              // trần alpha overlay
        _Invert  ("Invert Direction", Float) = 0              // 0 = uv.y=0 ở phía tile đè
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #pragma require 2darray
            #include "UnityCG.cginc"

            UNITY_DECLARE_TEX2DARRAY(_Tiles);
            fixed4 _Tint;
            float  _Feather;
            float  _Alpha;
            float  _Invert;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0; // uv.y: 0 ở phía tile "đè", 1 về phía ngoài dải
                float2 uv2    : TEXCOORD1; // x = center layer index
                fixed4 color  : COLOR;     // optional tint per-edge
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float  li  : TEXCOORD1; // layer index
                fixed4 col : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                o.li  = v.uv2.x;
                o.col = v.color * _Tint;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                int layer = (int)round(i.li);      // i.li = uv2.x = blendLayer
                fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_Tiles, float3(i.uv, layer));
                float t = smoothstep(0,1, saturate(i.uv.y/_Feather));
                if (_Invert > 0.5) t = 1 - t;
                c.a *= (1 - t) * _Alpha;           // chỉ giảm alpha ra mép
                return c;
            }
            ENDCG
        }
    }
}
