Shader "Construction/SC2Ghost"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0,1,1,0.25)
        _InvalidColor ("Invalid Color", Color) = (1,0.2,0.2,0.35)

        _PulseSpeed ("Pulse Speed", Float) = 3
        _PulseStrength ("Pulse Strength", Float) = 0.3

        _GridScale ("Grid Scale", Float) = 4
        _GridWidth ("Grid Width", Float) = 0.05

        _DepthFade ("Depth Fade", Float) = 1.5
        _Valid ("Valid Placement", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            float4 _BaseColor;
            float4 _InvalidColor;

            float _PulseSpeed;
            float _PulseStrength;

            float _GridScale;
            float _GridWidth;

            float _DepthFade;
            float _Valid;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);

                OUT.positionCS = pos.positionCS;
                OUT.worldPos = pos.positionWS;

                VertexNormalInputs normal = GetVertexNormalInputs(IN.normalOS);
                OUT.normalWS = normal.normalWS;

                return OUT;
            }

            float grid(float3 p)
            {
                float3 g = abs(frac(p * _GridScale) - 0.5);
                float3 line_ = smoothstep(_GridWidth, 0, g);
                return max(max(line_.x, line_.y), line_.z);
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float3 world = IN.worldPos;

                float4 color = lerp(_InvalidColor, _BaseColor, _Valid);

                // energy grid
                float g = grid(world);
                color.rgb += g * 0.6;

                // pulse
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                color.rgb += pulse * _PulseStrength;

                // fresnel
                float3 viewDir = normalize(_WorldSpaceCameraPos - world);
                float fresnel = pow(1 - saturate(dot(viewDir, normalize(IN.normalWS))), 3);

                color.rgb += fresnel * 0.8;

                color.a *= 0.8 + fresnel * 0.4;

                return color;
            }

            ENDHLSL
        }
    }
}