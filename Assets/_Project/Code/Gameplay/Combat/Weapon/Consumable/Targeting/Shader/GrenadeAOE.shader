Shader "Custom/GrenadeAOE"
{
    Properties
    {
        _InnerRadius    ("Inner Radius",      Float) = 2
        _OuterRadius    ("Outer Radius",      Float) = 5

        _InnerColor     ("Inner Color",       Color) = (1, 0.3, 0.1, 0.5)
        _OuterColor     ("Outer Color",       Color) = (0.1, 0.7, 1, 0.3)

        _EdgeThickness  ("Edge Thickness",    Float) = 0.08

        // Кольцо внешнего радиуса
        _RingWidth      ("Ring Width",        Float) = 0.06
        _RingColor      ("Ring Color",        Color) = (0.1, 0.8, 1, 1)
        _SegmentCount   ("Segment Count",     Float) = 24
        _SegmentGap     ("Segment Gap",       Float) = 0.08   // доля от сегмента
        _RingRotSpeed   ("Ring Rot Speed",    Float) = 0.4    // рад/сек

        // Кольцо внутреннего радиуса
        _InnerRingWidth ("Inner Ring Width",  Float) = 0.05
        _InnerRingColor ("Inner Ring Color",  Color) = (1, 0.3, 0.1, 1)
        _InnerSegCount  ("Inner Seg Count",   Float) = 16
        _InnerRotSpeed  ("Inner Rot Speed",   Float) = -0.3

        // Радиальная метка (тик) на внешнем кольце
        _TickLength     ("Tick Length",       Float) = 0.15
        _TickWidth      ("Tick Width",        Float) = 0.015

        // Пульс
        _PulseSpeed     ("Pulse Speed",       Float) = 2.0
        _PulseAmp       ("Pulse Amplitude",   Float) = 0.25

        // Expanding pulse ring
        _PulseRingSpeed ("Pulse Ring Speed",  Float) = 1.2
        _PulseRingWidth ("Pulse Ring Width",  Float) = 0.04

        // Scan sweep
        _ScanSpeed      ("Scan Speed",        Float) = 0.8
        _ScanColor      ("Scan Color",        Color) = (0.1, 0.8, 1, 0.5)
        _ScanArcAngle   ("Scan Arc Angle",    Float) = 0.4  // рад
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
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

            float  _InnerRadius, _OuterRadius;
            float4 _InnerColor, _OuterColor;
            float  _EdgeThickness;

            float  _RingWidth, _SegmentCount, _SegmentGap, _RingRotSpeed;
            float4 _RingColor;

            float  _InnerRingWidth, _InnerSegCount, _InnerRotSpeed;
            float4 _InnerRingColor;

            float  _TickLength, _TickWidth;

            float  _PulseSpeed, _PulseAmp;
            float  _PulseRingSpeed, _PulseRingWidth;
            float  _ScanSpeed, _ScanArcAngle;
            float4 _ScanColor;

            struct appdata { float4 vertex : POSITION; };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            // ---- Helpers ----

            // Угол в [-PI, PI]
            float Atan2Safe(float y, float x)
            {
                return atan2(y, x);
            }

            // Антиалиасинг — размытая граница ±1px в мировых единицах
            float AA(float dist, float edge, float width)
            {
                return smoothstep(edge + width, edge - width, dist);
            }

            // Рисует дугу: внешний радиус arc_r, ширина ring_w
            // Сегментация: numSeg сегментов, gapFrac = доля зазора [0..1]
            // angle_offset — поворот всего кольца
            float SegmentedRing(float dist, float2 dir, float arc_r,
                                float ring_w, float numSeg, float gapFrac,
                                float angle_offset, float aa_px)
            {
                float inRing = AA(dist, arc_r - ring_w, aa_px)
                             * AA(arc_r,   dist,         aa_px);
                if (inRing < 0.001) return 0;

                float angle    = Atan2Safe(dir.y, dir.x) + angle_offset;
                float segAngle = UNITY_TWO_PI / numSeg;
                float gapAngle = segAngle * gapFrac;
                float mod_a    = fmod(angle + UNITY_TWO_PI * 2.0, segAngle);
                float inSeg    = smoothstep(gapAngle * 0.5 - 0.01,
                                            gapAngle * 0.5 + 0.01,
                                            abs(mod_a - segAngle * 0.5));
                return inRing * inSeg;
            }

            // Тики на 4 кардинальных направлениях
            float CardinalTicks(float2 localDir, float dist, float arc_r,
                                float tickLen, float tickW, float aa_px)
            {
                float result = 0;
                float2 dirs[4];
                dirs[0] = float2( 1,  0);
                dirs[1] = float2(-1,  0);
                dirs[2] = float2( 0,  1);
                dirs[3] = float2( 0, -1);

                for (int i = 0; i < 4; i++)
                {
                    float along = dot(localDir * dist, dirs[i]);
                    float perp  = abs(dot(localDir * dist,
                                float2(-dirs[i].y, dirs[i].x)));
                    float inLen  = step(arc_r - tickLen, along)
                                 * step(along, arc_r + tickLen * 0.5);
                    float inPerp = smoothstep(tickW * 0.5 + 0.015,
                                             tickW * 0.5 - 0.015,
                                             perp);
                    result = max(result, inLen * inPerp);
                }
                return result;
            }

            // Expanding pulse ring
            float PulseRing(float dist, float inner_r, float outer_r,
                            float speed, float ring_w, float aa_px)
            {
                float phase = frac(_Time.y * speed);
                float ring_r = lerp(inner_r, outer_r, phase);
                float alpha  = 1.0 - phase; // fade out as it expands
                float inRing = AA(dist, ring_r - ring_w * 0.5, aa_px)
                             * AA(ring_r + ring_w * 0.5, dist, aa_px);
                return inRing * alpha;
            }

            // Scan sweep (2D radar)
            float ScanSweep(float2 dir, float dist, float outer_r,
                            float speed, float arc, float aa_px)
            {
                float scanAngle = _Time.y * speed;
                float pixAngle  = Atan2Safe(dir.y, dir.x);
                float delta     = pixAngle - scanAngle;
                // Нормализуем в [-PI, PI]
                delta = fmod(delta + UNITY_PI * 3.0, UNITY_TWO_PI) - UNITY_PI;

                float inArc  = smoothstep(0, -arc, delta) // от 0 до -arc назад
                             * step(0, dist)
                             * step(dist, outer_r);
                float fade   = saturate(-delta / arc);    // ярче у передней границы
                return inArc * fade;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 center   = float2(unity_ObjectToWorld._m03,
                                         unity_ObjectToWorld._m23);
                float2 pos      = i.worldPos.xz;
                float2 offset   = pos - center;
                float  dist     = length(offset);
                float2 dir      = (dist > 0.001) ? offset / dist : float2(1, 0);

                float aa_px = 0.035; // антиалиасинг ~1px в единицах мира

                if (dist > _OuterRadius + _TickLength + 0.1)
                    discard;

                float  pulse    = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;

                // ---------- Заливки ----------

                float4 col = float4(0, 0, 0, 0);

                // Внутренняя зона
                if (dist <= _InnerRadius)
                {
                    float alpha = _InnerColor.a * (1.0 - 0.15 * pulse);
                    col = float4(_InnerColor.rgb, alpha);
                }
                else if (dist <= _OuterRadius)
                {
                    // Внешняя зона — радиальный fade к краям
                    float t  = (dist - _InnerRadius) / (_OuterRadius - _InnerRadius);
                    float eO = smoothstep(_OuterRadius,
                                          _OuterRadius - _EdgeThickness, dist);
                    float eI = smoothstep(_InnerRadius,
                                          _InnerRadius + _EdgeThickness, dist);
                    float a  = _OuterColor.a * eO * eI;
                    col      = float4(_OuterColor.rgb, a);
                }

                // ---------- Scan sweep ----------
                float scanV = ScanSweep(dir, dist, _OuterRadius,
                                        _ScanSpeed, _ScanArcAngle, aa_px);
                col.rgb  = lerp(col.rgb, _ScanColor.rgb, scanV * _ScanColor.a);
                col.a    = max(col.a, scanV * 0.2);

                // ---------- Expanding pulse ring ----------
                float prV = PulseRing(dist, _InnerRadius, _OuterRadius,
                                      _PulseRingSpeed, _PulseRingWidth, aa_px);
                col.rgb = lerp(col.rgb, _RingColor.rgb, prV * 0.6);
                col.a   = max(col.a, prV * 0.5);

                // ---------- Внешнее сегментированное кольцо ----------
                float rotO = _Time.y * _RingRotSpeed;
                float ringV = SegmentedRing(dist, dir, _OuterRadius,
                                            _RingWidth, _SegmentCount,
                                            _SegmentGap, rotO, aa_px);
                float ringAlpha = ringV * _RingColor.a
                                * (1.0 + _PulseAmp * pulse);
                col = lerp(col, float4(_RingColor.rgb, 1), ringAlpha);
                col.a = max(col.a, ringAlpha);

                // ---------- Внутреннее сегментированное кольцо ----------
                float rotI = _Time.y * _InnerRotSpeed;
                float iRingV = SegmentedRing(dist, dir, _InnerRadius,
                                             _InnerRingWidth, _InnerSegCount,
                                             _SegmentGap, rotI, aa_px);
                float iRingAlpha = iRingV * _InnerRingColor.a
                                 * (1.0 + _PulseAmp * pulse);
                col = lerp(col, float4(_InnerRingColor.rgb, 1), iRingAlpha);
                col.a = max(col.a, iRingAlpha);

                // ---------- Тики на внешнем кольце ----------
                float tickV = CardinalTicks(dir, dist, _OuterRadius,
                                            _TickLength, _TickWidth, aa_px);
                col = lerp(col, float4(_RingColor.rgb, 1), tickV * 0.9);
                col.a = max(col.a, tickV * 0.8);

                // ---------- Финальный clamp ----------
                col.a = saturate(col.a);
                if (col.a < 0.005) discard;

                return col;
            }
            ENDCG
        }
    }
}