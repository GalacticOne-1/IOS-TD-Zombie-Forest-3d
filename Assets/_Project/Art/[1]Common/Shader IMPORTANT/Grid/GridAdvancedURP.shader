Shader "Construction/GridAdvancedURP"
{
    Properties
    {
        _CellSize ("Cell Size", Float) = 1
        _MajorStep ("Major Step", Float) = 5

        _LineWidth ("Minor Line Width", Float) = 0.02
        _MajorLineWidth ("Major Line Width", Float) = 0.05

        _LineColor ("Minor Line Color", Color) = (0,1,0,0.6)
        _MajorColor ("Major Line Color", Color) = (0,1,0,1)

        _BaseColor ("Base Color", Color) = (0,0,0,0)

        _CornerSize ("Corner Size", Float) = 0.15
        _CornerWidth ("Corner Width", Float) = 0.03
        _CornerColor ("Corner Color", Color) = (1,1,1,1)

        _HoverCell ("Hover Cell", Vector) = (0,0,0,0)
        _HoverColor ("Hover Color", Color) = (1,1,0,0.25)

        _FootprintMin ("Footprint Min", Vector) = (0,0,0,0)
        _FootprintMax ("Footprint Max", Vector) = (0,0,0,0)
        _FootprintColor ("Footprint Color", Color) = (0,1,1,0.25)

        _OriginMin ("Origin Min", Vector) = (0,0,0,0)
        _OriginMax ("Origin Max", Vector) = (0,0,0,0)
        _OriginColor ("Origin Color", Color) = (0.3,0.8,1,0.25)

        _GridOffset ("Grid Offset", Vector) = (0,0,0,0)
        _GridVisible ("Grid Visible", Float) = 1

        // NEW: static blocked areas
        _BlockedMask ("Blocked Mask", 2D) = "black" {}
        _BlockedColor ("Blocked Color", Color) = (1,0,0,0.2)
    }

    SubShader
    {
        Tags{ "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
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
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float _CellSize;
            float _MajorStep;

            float _LineWidth;
            float _MajorLineWidth;

            float4 _LineColor;
            float4 _MajorColor;
            float4 _BaseColor;

            float _CornerSize;
            float _CornerWidth;
            float4 _CornerColor;

            float4 _HoverCell;
            float4 _HoverColor;

            float4 _FootprintMin;
            float4 _FootprintMax;
            float4 _FootprintColor;

            float4 _OriginMin;
            float4 _OriginMax;
            float4 _OriginColor;

            float4 _GridOffset;
            float _GridVisible;

            // NEW
            TEXTURE2D(_BlockedMask);
            SAMPLER(sampler_BlockedMask);
            float4 _BlockedMask_TexelSize; // auto-filled by Unity: x=1/w, y=1/h, z=w, w=h
            float4 _BlockedColor;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);

                OUT.positionCS = pos.positionCS;
                OUT.worldPos = pos.positionWS;

                return OUT;
            }

            float gridLine(float coord, float width)
            {
                float g = frac(coord / _CellSize);
                g = min(g, 1 - g);

                float aa = fwidth(coord);

                return smoothstep(width + aa, width - aa, g);
            }

            float majorLine(float coord)
            {
                float stepSize = _CellSize * _MajorStep;

                float g = frac(coord / stepSize);
                g = min(g, 1 - g);

                float aa = fwidth(coord);

                return smoothstep(_MajorLineWidth + aa, _MajorLineWidth - aa, g);
            }

            float cornerBracket(float2 uv)
            {
                float2 edge = min(uv, 1 - uv);

                float lineX = smoothstep(_CornerWidth, 0, edge.x);
                float lineY = smoothstep(_CornerWidth, 0, edge.y);

                float cornerMask =
                    step(edge.x, _CornerSize) *
                    step(edge.y, _CornerSize);

                return max(lineX, lineY) * cornerMask;
            }

            // NEW: samples the blocked mask for a given cell coordinate.
            // One texel == one construction cell (see runtime texture generator).
            float blockedMask(float2 cell)
            {
                float2 texUV = (cell + 0.5) * _BlockedMask_TexelSize.xy;
                return step(0.5, SAMPLE_TEXTURE2D(_BlockedMask, sampler_BlockedMask, texUV).r);
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 world = IN.worldPos.xz - _GridOffset.xy;

                // grid
                float gx = gridLine(world.x, _LineWidth);
                float gz = gridLine(world.y, _LineWidth);

                float mx = majorLine(world.x);
                float mz = majorLine(world.y);

                float minor = max(gx, gz);
                float major = max(mx, mz);

                float4 color = lerp(_BaseColor, _LineColor, minor);
                color = lerp(color, _MajorColor, major);

                // cell coordinates
                float2 cell = floor(world / _CellSize);
                float2 local = frac(world / _CellSize);

                // corners
                float corner = cornerBracket(local);

                color.rgb = lerp(color.rgb, _CornerColor.rgb, corner * _CornerColor.a);
                color.a = max(color.a, corner * _CornerColor.a);

                // ===== NEW: blocked areas (static) =====
                // sits beneath origin/footprint/hover, above grid+corners
                float blocked = blockedMask(cell);
                color = lerp(color, _BlockedColor, blocked * _BlockedColor.a);

                // origin mask (old building position)
                float ox =
                    step(_OriginMin.x, cell.x) *
                    step(cell.x, _OriginMax.x - 1);

                float oy =
                    step(_OriginMin.y, cell.y) *
                    step(cell.y, _OriginMax.y - 1);

                float originMask = ox * oy;

                color = lerp(color, _OriginColor, originMask * _OriginColor.a);

                // footprint mask
                float fx =
                    step(_FootprintMin.x, cell.x) *
                    step(cell.x, _FootprintMax.x - 1);

                float fy =
                    step(_FootprintMin.y, cell.y) *
                    step(cell.y, _FootprintMax.y - 1);

                float footprintMask = fx * fy;

                color = lerp(color, _FootprintColor, footprintMask * _FootprintColor.a);

                // hover mask
                float hoverMask =
                    step(abs(cell.x - _HoverCell.x), 0.5) *
                    step(abs(cell.y - _HoverCell.y), 0.5);

                color = lerp(color, _HoverColor, hoverMask * _HoverColor.a);

                color.a *= _GridVisible;

                return color;
            }

            ENDHLSL
        }
    }
}