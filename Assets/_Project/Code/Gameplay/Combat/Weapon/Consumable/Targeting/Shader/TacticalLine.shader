Shader "Custom/TacticalLine"
{
    Properties
    {
        _Tiling ("Tiling", Float) = 1
        _ColorValid     ("Valid Color",       Color) = (0.1, 0.9, 1, 1)
        _ColorInvalid   ("Invalid Color",     Color) = (1, 0.15, 0.1, 1)
        _Valid          ("Is Valid",          Float) = 1

        _DashLength     ("Dash Length",       Float) = 0.18
        _GapLength      ("Gap Length",        Float) = 0.10
        _ScrollSpeed    ("Scroll Speed",      Float) = 1.2

        _EdgeFade       ("Edge Fade Width",   Float) = 0.15
        _Brightness     ("Brightness",        Float) = 1.4

        _GlowWidth      ("Glow Width",        Float) = 0.6
        _GlowIntensity  ("Glow Intensity",    Float) = 0.35
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Tiling;
            float4 _ColorValid, _ColorInvalid;
            float  _Valid;
            float  _DashLength, _GapLength, _ScrollSpeed;
            float  _EdgeFade, _Brightness;
            float  _GlowWidth, _GlowIntensity;

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uv    = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // uv.x — вдоль линии [0..1], uv.y — поперёк [0..1]
                float along = i.uv.x;
                float across = i.uv.y - 0.5; // [-0.5 .. 0.5]

                // Прокрутка дашей от юнита к цели
                float totalLength = _DashLength + _GapLength;
                float worldUV = i.uv.x * _Tiling;
                float scrolled = frac((worldUV - _Time.y * _ScrollSpeed) / totalLength);
                float dash = step(_GapLength / totalLength, scrolled);

                // Fade у начала и конца линии
                float fadeStart = smoothstep(0.0, _EdgeFade, along);
                float fadeEnd   = smoothstep(1.0, 1.0 - _EdgeFade, along);
                float edgeFade  = fadeStart * fadeEnd;

                // Поперечный glow — ярче по центру
                float glow = exp(-abs(across) / _GlowWidth * 4.0) * _GlowIntensity;

                float4 col = lerp(_ColorInvalid, _ColorValid, saturate(_Valid));
                col.rgb *= _Brightness;

                // Основная линия + glow
                float alpha = (dash + glow) * edgeFade * col.a;

                // Пульс яркости у дашей
                float pulse = sin(_Time.y * 3.0) * 0.08 + 0.92;
                alpha *= pulse;

                return float4(col.rgb, saturate(alpha));
            }
            ENDCG
        }
    }
}