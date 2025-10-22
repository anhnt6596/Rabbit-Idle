Shader "Tiles/UnlitArray"
{
    Properties
    {
        _Tiles ("Texture2DArray", 2DArray) = "" {}
        _Tint ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            UNITY_DECLARE_TEX2DARRAY(_Tiles);
            fixed4 _Tint;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float2 uv2    : TEXCOORD1; // x = layer index
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                float  li  : TEXCOORD1; // layer index (float)
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.li = v.uv2.x;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                int layer = (int)round(i.li);
                fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_Tiles, float3(i.uv, layer));
                return col * _Tint;
            }
            ENDCG
        }
    }
}
