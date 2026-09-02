Shader "Construction/GridCellURP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.2,0.2,0.2,0.25)
        _LineColor ("Line Color", Color) = (1,1,1,0.8)
        _InvalidColor ("Invalid Color", Color) = (1,0,0,0.4)

        _LineWidth ("Line Width", Range(0.001,0.1)) = 0.02
        _CornerThickness ("Corner Thickness", Range(0.001,0.2)) = 0.05
        _CornerSize ("Corner Size", Range(0.01,0.5)) = 0.2

        _IsValid ("Is Valid", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "Grid"

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _BaseColor;
            float4 _LineColor;
            float4 _InvalidColor;

            float _LineWidth;
            float _CornerThickness;
            float _CornerSize;
            float _IsValid;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float Border(float2 uv, float width)
            {
                float mask = 0;
                mask += step(uv.x, width);
                mask += step(uv.y, width);
                mask += step(1 - uv.x, width);
                mask += step(1 - uv.y, width);
                return saturate(mask);
            }

            float Corner(float2 uv)
            {
                float mask = 0;

                float2 tl = float2(uv.x, 1-uv.y);
                float2 tr = float2(1-uv.x, 1-uv.y);
                float2 bl = float2(uv.x, uv.y);
                float2 br = float2(1-uv.x, uv.y);

                float corner;

                corner = step(bl.x, _CornerSize) * step(bl.y, _CornerThickness)
                       + step(bl.y, _CornerSize) * step(bl.x, _CornerThickness);
                mask += corner;

                corner = step(br.x, _CornerSize) * step(br.y, _CornerThickness)
                       + step(br.y, _CornerSize) * step(br.x, _CornerThickness);
                mask += corner;

                corner = step(tl.x, _CornerSize) * step(tl.y, _CornerThickness)
                       + step(tl.y, _CornerSize) * step(tl.x, _CornerThickness);
                mask += corner;

                corner = step(tr.x, _CornerSize) * step(tr.y, _CornerThickness)
                       + step(tr.y, _CornerSize) * step(tr.x, _CornerThickness);
                mask += corner;

                return saturate(mask);
            }

            half4 frag (Varyings i) : SV_Target
            {
                float2 uv = i.uv;

                float border = Border(uv, _LineWidth);
                float corners = Corner(uv);

                float mask = max(border, corners);

                float4 cellColor = lerp(_BaseColor, _InvalidColor, 1 - _IsValid);

                float4 finalColor = lerp(cellColor, _LineColor, mask);

                return finalColor;
            }

            ENDHLSL
        }
    }
}