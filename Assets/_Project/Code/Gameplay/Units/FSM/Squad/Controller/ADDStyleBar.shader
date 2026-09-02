Shader "UI/ADDStyleBar"
{
    Properties
    {
        _Fill ("Fill", Range(0,1)) = 1
        _ColorA ("Color Start", Color) = (0,1,1,1)
        _ColorB ("Color End", Color) = (0,0.3,1,1)

        _Glow ("Glow Strength", Range(0,5)) = 2

        _ScanSpeed ("Scan Speed", Range(0,10)) = 3
        _ScanStrength ("Scan Strength", Range(0,1)) = 0.25

        _Edge ("Edge Sharpness", Range(0.001,0.1)) = 0.01
        
        _ScanLines ("Scan Lines", Range(1,100)) = 40
        _LenghtLine ("Lenght Line (pixels)", Float) = 200
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Blend SrcAlpha One
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            float _Fill;
            float4 _ColorA;
            float4 _ColorB;

            float _Glow;

            float _ScanSpeed;
            float _ScanStrength;

            float _Edge;
            float _ScanLines;
            float _LenghtLine;


            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                float x = i.uv.x;

                float4 col = lerp(_ColorA, _ColorB, frac(x)); // frac — потому что UV теперь 0..N

                float phase = x * _ScanLines + _Time.y * _ScanSpeed;
                float scan = sin(phase * 6.28318);
                scan = scan * _ScanStrength + 1;

                col.rgb *= scan;
                col.rgb *= _Glow;

                return col;
            }

            ENDHLSL
        }
    }
}