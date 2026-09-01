Shader "FateloomFx/StylizedStarUnlit"
{
    Properties
    {
        [Header(BASE)]
        [Toggle(_USE_TEXTURE)] _UseTexture ("Use Texture", Float) = 0
        _MainTex ("Texture (Optional)", 2D) = "white" {}
        _Color ("Base Color", Color) = (1, 0.85, 0.2, 1)
        _Brightness ("Brightness", Range(0, 10)) = 1.5

        [Header(FRESNEL RIM)]
        [Toggle(_FRESNEL_ON)] _FresnelEnabled ("Enable Fresnel", Float) = 1
        _FresnelColor ("Fresnel Color", Color) = (0.4, 0.7, 1.0, 1)
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 3.0
        _FresnelIntensity ("Fresnel Intensity", Range(0, 10)) = 2.0

        [Header(INNER GLOW)]
        [Toggle(_INNERGLOW_ON)] _InnerGlowEnabled ("Enable Inner Glow", Float) = 1
        _InnerGlowColor ("Inner Glow Color", Color) = (1, 0.6, 0.1, 1)
        _InnerGlowPower ("Inner Glow Power", Range(0.1, 10)) = 2.0
        _InnerGlowIntensity ("Inner Glow Intensity", Range(0, 5)) = 0.8

        [Header(EMISSION)]
        [HDR] _EmissionColor ("Emission Color", Color) = (1, 0.8, 0.2, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 15)) = 1.0

        [Header(PULSE ANIMATION)]
        [Toggle(_PULSE_ON)] _PulseEnabled ("Enable Pulse", Float) = 0
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2.0
        _PulseMin ("Pulse Min", Range(0, 1)) = 0.7
        _PulseMax ("Pulse Max", Range(1, 3)) = 1.3

        [Header(HEIGHT GRADIENT)]
        [Toggle(_GRADIENT_ON)] _GradientEnabled ("Enable Height Gradient", Float) = 0
        _GradientTopColor ("Top Color", Color) = (1, 1, 0.5, 1)
        _GradientBottomColor ("Bottom Color", Color) = (1, 0.3, 0.05, 1)
        _GradientOffset ("Gradient Offset", Range(-2, 2)) = 0
        _GradientRange ("Gradient Range", Range(0.01, 5)) = 1.0

        [Header(RENDERING)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
        [Enum(Off,0,On,1)] _ZWrite ("ZWrite", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Blend [_SrcBlend] [_DstBlend]
        Cull [_Cull]
        ZWrite [_ZWrite]

        Pass
        {
            Name "StylizedStarUnlit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma shader_feature_local _USE_TEXTURE
            #pragma shader_feature_local _FRESNEL_ON
            #pragma shader_feature_local _INNERGLOW_ON
            #pragma shader_feature_local _PULSE_ON
            #pragma shader_feature_local _GRADIENT_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 positionOS : TEXCOORD3;
                float4 vertexColor : COLOR;
                float fogFactor : TEXCOORD4;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _Brightness;

                half4 _FresnelColor;
                half _FresnelPower;
                half _FresnelIntensity;

                half4 _InnerGlowColor;
                half _InnerGlowPower;
                half _InnerGlowIntensity;

                half4 _EmissionColor;
                half _EmissionIntensity;

                half _PulseSpeed;
                half _PulseMin;
                half _PulseMax;

                half4 _GradientTopColor;
                half4 _GradientBottomColor;
                half _GradientOffset;
                half _GradientRange;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;

                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(v.normalOS);

                o.positionCS = posInputs.positionCS;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normalWS = normInputs.normalWS;
                o.viewDirWS = GetWorldSpaceNormalizeViewDir(posInputs.positionWS);
                o.positionOS = v.positionOS.xyz;
                o.vertexColor = v.color;
                o.fogFactor = ComputeFogFactor(posInputs.positionCS.z);

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half3 normal = normalize(i.normalWS);
                half3 viewDir = normalize(i.viewDirWS);

                // ── Base Color ──
                half4 baseColor = _Color * i.vertexColor;

                #ifdef _USE_TEXTURE
                    half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                    baseColor *= tex;
                #endif

                // ── Height Gradient ──
                #ifdef _GRADIENT_ON
                    half heightFactor = saturate((i.positionOS.y + _GradientOffset) / _GradientRange * 0.5 + 0.5);
                    half4 gradColor = lerp(_GradientBottomColor, _GradientTopColor, heightFactor);
                    baseColor.rgb *= gradColor.rgb;
                #endif

                half NdotV = saturate(dot(normal, viewDir));

                // ── Fresnel Rim ──
                half fresnel = 0;
                #ifdef _FRESNEL_ON
                    fresnel = pow(1.0 - NdotV, _FresnelPower) * _FresnelIntensity;
                #endif

                // ── Inner Glow ──
                half innerGlow = 0;
                #ifdef _INNERGLOW_ON
                    innerGlow = pow(NdotV, _InnerGlowPower) * _InnerGlowIntensity;
                #endif

                // ── Pulse ──
                half pulseMul = 1.0;
                #ifdef _PULSE_ON
                    half wave = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                    pulseMul = lerp(_PulseMin, _PulseMax, wave);
                #endif

                // ── Final ──
                half3 finalColor = baseColor.rgb * _Brightness;

                #ifdef _FRESNEL_ON
                    finalColor += _FresnelColor.rgb * fresnel;
                #endif

                #ifdef _INNERGLOW_ON
                    finalColor += _InnerGlowColor.rgb * innerGlow;
                #endif

                finalColor += _EmissionColor.rgb * _EmissionIntensity;
                finalColor *= pulseMul;

                half4 col = half4(finalColor, baseColor.a);
                col.rgb = MixFog(col.rgb, i.fogFactor);

                return col;
            }
            ENDHLSL
        }

        // Depth & Shadow pass for proper rendering
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings DepthVert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }

            half4 DepthFrag(Varyings i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
