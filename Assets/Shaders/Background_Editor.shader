Shader "CustomRenderTexture/Background_Editor"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BlurSize("Blur Size", Float) = 2.0
        _TintColor("Tint Color", Color) = (1,1,1,0.3)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _TintColor;
            float _BlurSize;
            float4 _MainTex_TexelSize;

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

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 col = float4(0,0,0,0);

                // 简单 9 点均值模糊
                for(int x=-1; x<=1; x++)
                {
                    for(int y=-1; y<=1; y++)
                    {
                        float2 offset = float2(x, y) * _BlurSize * _MainTex_TexelSize.xy;
                        col += tex2D(_MainTex, uv + offset);
                    }
                }
                col /= 9.0;

                // 添加半透明色调
                col.rgb = lerp(col.rgb, _TintColor.rgb, _TintColor.a);

                return col;
            }
            ENDCG
        }
    }
}
