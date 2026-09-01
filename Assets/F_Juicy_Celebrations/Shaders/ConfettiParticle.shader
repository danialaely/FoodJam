Shader "FateloomFx/ConfettiParticle"
{
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Brightness ("Brightness", Range(0, 3)) = 1.2
        _FlipSpeed ("Flip Speed", Range(0, 20)) = 8.0
        _Softness ("Edge Softness", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
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
                float4 customData : TEXCOORD1; // particle custom data (random seed)
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Brightness;
            float _FlipSpeed;
            float _Softness;

            v2f vert(appdata v)
            {
                v2f o;

                // Konfeti "flip" efekti - vertex'i zaman bazlı scale ederek döndürme illüzyonu
                float seed = v.customData.x; // her parçacık için farklı seed
                float flipPhase = _Time.y * _FlipSpeed + seed * 6.2831;
                float flipScale = abs(sin(flipPhase)); // 0-1 arası, kart çevirme efekti

                // UV'nin X ekseninde scale
                float2 centeredUV = v.uv - 0.5;
                centeredUV.x *= lerp(0.15, 1.0, flipScale); // tamamen kapanmasın
                v.uv = centeredUV + 0.5;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 col = tex * i.color * _Color;
                col.rgb *= _Brightness;

                // Kenar yumuşatma
                float2 edgeDist = abs(i.uv - 0.5) * 2.0;
                float edgeFade = 1.0 - smoothstep(1.0 - _Softness, 1.0, max(edgeDist.x, edgeDist.y));
                col.a *= edgeFade;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Particles/Alpha Blended"
}
