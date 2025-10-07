Shader "Unlit/RoundedRectWithOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Radius", Range(0, 1)) = 0.1
        _Color ("Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.5)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Radius;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // 圆角矩形距离函数
            float roundedBox(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                float2 uv = i.uv - 0.5;
                float2 rectSize = float2(0.5, 0.5) - _Radius;
                
                // 计算到圆角矩形的距离（正数表示在外部，负数表示在内部）
                float distance = roundedBox(uv, rectSize, _Radius);
                
                // 主矩形区域
                if (distance < 0)
                {
                    // 在主矩形内部，保持原有颜色
                    return col;
                }
                // 描边区域
                else if (distance < _OutlineWidth)
                {
                    // 在描边宽度范围内
                    return _OutlineColor;
                }
                // 外部区域
                else
                {
                    // 透明
                    col.a = 0;
                    return col;
                }
            }
            ENDCG
        }
    }
}
