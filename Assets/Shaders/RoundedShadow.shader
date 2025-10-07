Shader "UI/RoundedShadow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _CornerRadius ("Corner Radius", Range(0, 0.5)) = 0.1
        _ShadowColor ("Shadow Color", Color) = (0,0,0,0.5)
        _ShadowBlur ("Shadow Blur", Range(0, 0.1)) = 0.05
        _ShadowOffset ("Shadow Offset", Vector) = (0.02, -0.02, 0, 0)
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord  : TEXCOORD0;
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 pos      : TEXCOORD1;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float _CornerRadius;
            fixed4 _ShadowColor;
            float _ShadowBlur;
            float2 _ShadowOffset;
            float4 _ClipRect;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                
                // 将位置映射到[-0.5,0.5]范围
                OUT.pos = IN.texcoord - 0.5;
                
                return OUT;
            }

            // 圆角矩形函数
            float roundedBox(float2 pos, float2 size, float radius)
            {
                float2 q = abs(pos) - size + radius;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - radius;
            }

            // 平滑步进函数
            float smoothstep(float edge0, float edge1, float x)
            {
                float t = clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
                return t * t * (3.0 - 2.0 * t);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 阴影计算
                float2 shadowPos = IN.pos - _ShadowOffset;
                float shadowDist = roundedBox(shadowPos, 0.5 - _ShadowBlur, _CornerRadius);
                float shadowAlpha = 1.0 - smoothstep(-_ShadowBlur, _ShadowBlur, shadowDist);
                shadowAlpha *= _ShadowColor.a;
                
                // 主形状计算
                float mainDist = roundedBox(IN.pos, 0.5, _CornerRadius);
                float mainAlpha = 1.0 - smoothstep(0.0, 0.01, mainDist);
                
                // 采样纹理
                fixed4 texColor = tex2D(_MainTex, IN.texcoord);
                
                // 组合颜色
                fixed4 col = IN.color * texColor;
                
                // 应用阴影（在背景上）
                col.rgb = lerp(_ShadowColor.rgb, col.rgb, mainAlpha);
                col.a = max(shadowAlpha * (1.0 - mainAlpha), col.a * mainAlpha);
                
                // UI裁剪
                col.a *= UnityGet2DClipping(IN.vertex.xy, _ClipRect);
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (col.a - 0.001);
                #endif
                
                return col;
            }
            ENDCG
        }
    }
}