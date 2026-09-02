Shader "Custom/UI/SpriteFlashOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _FlashColor ("Flash Color", Color) = (1,1,1,1)
        _FlashAmount ("Flash Amount", Range(0,1)) = 0

        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size (px)", Range(0,10)) = 1

        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilComp ("Stencil Comparison", Float) = 8
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }

            ColorMask [_ColorMask]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define OUTLINE_SAMPLES 16
            #define PI 3.14159265359

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 _Color;
            fixed4 _FlashColor;
            fixed4 _OutlineColor;

            float _FlashAmount;
            float _OutlineSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.uv;
                OUT.color = IN.color * _Color;

                return OUT;
            }

            inline fixed4 SampleSprite(float2 uv)
            {
                return tex2D(_MainTex, uv);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 col = SampleSprite(IN.uv) * IN.color;

                // Flash
                col.rgb = lerp(col.rgb, _FlashColor.rgb, _FlashAmount);

                // Нет outline
                if (_OutlineSize <= 0.001)
                {
                    col.rgb *= col.a;
                    return col;
                }

                // Только прозрачные пиксели могут стать outline
                if (col.a < 0.01)
                {
                    float alpha = 0;

                    float2 radius = _MainTex_TexelSize.xy * _OutlineSize;

                    [unroll]
                    for (int i = 0; i < OUTLINE_SAMPLES; i++)
                    {
                        float angle = (2.0 * PI / OUTLINE_SAMPLES) * i;

                        float2 dir = float2(cos(angle), sin(angle));

                        alpha = max(alpha,
                            SampleSprite(IN.uv + dir * radius).a);
                    }

                    if (alpha > 0.01)
                        return _OutlineColor;
                }

                col.rgb *= col.a;
                return col;
            }

            ENDCG
        }
    }
}