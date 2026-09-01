Shader "FateloomFx/GodRays"
{
    Properties
    {
        _MainTex ("Ray Texture", 2D) = "white" {}
        _Color ("Ray Color", Color) = (1, 0.95, 0.7, 0.6)
        _CoreColor ("Core Glow Color", Color) = (1, 1, 0.85, 1)
        _Intensity ("Intensity", Range(0, 5)) = 1.5
        _FadeStart ("Fade Start (center)", Range(0, 1)) = 0.1
        _FadeEnd ("Fade End (edge)", Range(0, 1)) = 0.95
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 0.8
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.2
        _RotationSpeed ("Rotation Speed", Range(-2, 2)) = 0.15
        _RayCount ("Ray Count", Range(2, 32)) = 12
        _RaySharpness ("Ray Sharpness", Range(0.1, 10)) = 2.0
        _RayWidth ("Ray Width", Range(0.01, 0.5)) = 0.15
        _NoiseScale ("Noise Scale", Range(0, 10)) = 3.0
        _NoiseSpeed ("Noise Speed", Range(0, 2)) = 0.3
        _VignetteStrength ("Vignette Strength", Range(0, 3)) = 1.5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+100"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Blend One One // Additive blending for glow
        Cull Off
        ZWrite Off
        ZTest Always

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
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _CoreColor;
            float _Intensity;
            float _FadeStart;
            float _FadeEnd;
            float _PulseSpeed;
            float _PulseAmount;
            float _RotationSpeed;
            float _RayCount;
            float _RaySharpness;
            float _RayWidth;
            float _NoiseScale;
            float _NoiseSpeed;
            float _VignetteStrength;

            // Basit hash-based noise
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f); // smoothstep

                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Merkeze göre UV
                float2 centeredUV = i.uv - 0.5;
                float dist = length(centeredUV);
                float angle = atan2(centeredUV.y, centeredUV.x);

                // Dönen ışın açısı
                float rotatedAngle = angle - _Time.y * _RotationSpeed;

                // Işın deseni oluştur
                float rayPattern = sin(rotatedAngle * _RayCount) * 0.5 + 0.5;
                rayPattern = pow(rayPattern, _RaySharpness);

                // Noise ile ışınlara varyasyon ekle
                float noiseVal = noise(float2(rotatedAngle * _NoiseScale, _Time.y * _NoiseSpeed));
                rayPattern *= lerp(0.7, 1.3, noiseVal);

                // Merkezden dışarı doğru azalan yoğunluk (radial gradient)
                float radialFade = 1.0 - smoothstep(_FadeStart, _FadeEnd, dist);

                // Vignette efekti
                float vignette = 1.0 - pow(dist * 2.0, _VignetteStrength);
                vignette = saturate(vignette);

                // Pulse (nefes) efekti
                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;

                // Core glow - merkezde parlak çekirdek
                float coreGlow = exp(-dist * 8.0) * 0.8;

                // Son renk hesaplaması
                float rayIntensity = rayPattern * radialFade * pulse * _Intensity;
                fixed4 col = _Color * rayIntensity;
                col += _CoreColor * coreGlow * _Intensity;
                col *= vignette;
                col *= i.color; // particle color desteği

                // Alpha da intensity'ye bağlı
                col.a = saturate(rayIntensity + coreGlow) * _Color.a * i.color.a;

                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
