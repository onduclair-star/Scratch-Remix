Shader "Custom/CircleShadow"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _Radius ("Radius", Range(0, 0.5)) = 0.3
        _ShadowColor ("Shadow Color", Color) = (0,0,0,0.5)
        _ShadowOffset ("Shadow Offset", Vector) = (0.02, -0.02, 0, 0)
        _ShadowBlur ("Shadow Blur", Range(0, 0.1)) = 0.02
        _ShadowSmoothness ("Shadow Smoothness", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "PreviewType"="Plane"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
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
            
            fixed4 _Color;
            float _Radius;
            fixed4 _ShadowColor;
            float2 _ShadowOffset;
            float _ShadowBlur;
            float _ShadowSmoothness;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            // 绘制圆形函数
            float drawCircle(float2 uv, float2 center, float radius, float blur)
            {
                float dist = distance(uv, center);
                return smoothstep(radius + blur, radius - blur, dist);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float2 shadowUV = i.uv - _ShadowOffset;
                
                // 绘制阴影
                float shadowAlpha = drawCircle(shadowUV, center, _Radius, _ShadowBlur);
                fixed4 shadow = fixed4(_ShadowColor.rgb, _ShadowColor.a * shadowAlpha);
                
                // 绘制主圆形
                float circleAlpha = drawCircle(i.uv, center, _Radius, _ShadowSmoothness * 0.01);
                fixed4 circle = fixed4(_Color.rgb, _Color.a * circleAlpha);
                
                // 混合阴影和圆形（使用预乘Alpha混合）
                fixed4 col = circle + shadow * (1 - circle.a);
                return col;
            }
            ENDCG
        }
    }
}