Shader "FateloomFx/StarBurst"
{
    Properties
    {
        _MainTex ("Star Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1, 0.9, 0.3, 1)
        _CoreColor ("Core Color", Color) = (1, 1, 1, 1)
        _Brightness ("Brightness", Range(0, 8)) = 3.0
        _GlowSize ("Glow Size", Range(0, 2)) = 0.8
        _SpikeCount ("Spike Count", Range(2, 12)) = 4
        _SpikeLength ("Spike Length", Range(0, 2)) = 1.2
        _SpikeSharpness ("Spike Sharpness", Range(0.1, 20)) = 6.0
        _FlickerSpeed ("Flicker Speed", Range(0, 20)) = 5.0
        _FlickerAmount ("Flicker Amount", Range(0, 1)) = 0.3
        _RotationSpeed ("Rotation Speed", Range(-10, 10)) = 2.0
        _FadeOutPower ("Fade Out Power", Range(0.1, 5)) = 2.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+50"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Blend One One // Additive blending
        Cull Off
        ZWrite Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_particles
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 customData : TEXCOORD1;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float customSeed : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _CoreColor;
            float _Brightness;
            float _GlowSize;
            float _SpikeCount;
            float _SpikeLength;
            float _SpikeSharpness;
            float _FlickerSpeed;
            float _FlickerAmount;
            float _RotationSpeed;
            float _FadeOutPower;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.customSeed = v.customData.x;
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 centeredUV = i.uv - 0.5;
                float dist = length(centeredUV);
                float angle = atan2(centeredUV.y, centeredUV.x);

                // Dönen yıldız spike'ları
                float rotAngle = angle + _Time.y * _RotationSpeed + i.customSeed * 6.2831;
                float spikes = pow(abs(cos(rotAngle * _SpikeCount * 0.5)), _SpikeSharpness);
                spikes *= _SpikeLength;

                // Radial glow - merkezden dışarı azalan parlaklık
                float glow = exp(-dist * (3.0 / max(_GlowSize, 0.01)));

                // Spike'ları mesafeye göre azalt
                float spikeGlow = spikes * exp(-dist * (2.0 / max(_SpikeLength, 0.01)));

                // Core - ortadaki parlak nokta
                float core = exp(-dist * 15.0);

                // Flicker (titreşim) efekti
                float flicker = 1.0 - _FlickerAmount * sin(_Time.y * _FlickerSpeed + i.customSeed * 10.0);

                // Toplam yoğunluk
                float intensity = (glow + spikeGlow + core * 2.0) * flicker * _Brightness;

                // Renk: core beyazımsı, dış kısım tint renginde
                fixed4 col = lerp(_Color, _CoreColor, core * 0.8);
                col.rgb *= intensity;
                col *= i.color; // particle system color modülü desteği

                // Alpha - fade out
                col.a = saturate(pow(intensity * 0.5, _FadeOutPower)) * i.color.a;

                // Kenarları tamamen şeffaf yap
                float edgeMask = 1.0 - smoothstep(0.4, 0.5, dist);
                col *= edgeMask;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
