Shader "FateloomFx/SoftAdditiveParticle"
{
    // Hem konfeti trails hem de genel parçacık efektleri için
    // kullanılabilecek çok amaçlı soft additive shader
    Properties
    {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0, 5)) = 1.0
        _SoftFactor ("Soft Particles Factor", Range(0, 3)) = 1.0
        [Toggle] _UseVertexColor ("Use Vertex Color", Float) = 1
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

        Blend One OneMinusSrcAlpha // Premultiplied alpha
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
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 projPos : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Intensity;
            float _SoftFactor;
            float _UseVertexColor;

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                o.projPos = ComputeScreenPos(o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Soft particles: sahne geometrisiyle kesişmeyi yumuşat
                float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                float partZ = i.projPos.z;
                float softFade = saturate(_SoftFactor * (sceneZ - partZ));

                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 col = tex * _Color * _Intensity;

                // Vertex color modülasyonu
                if (_UseVertexColor > 0.5)
                    col *= i.color;

                // Premultiplied alpha
                col.rgb *= col.a;
                col *= softFade;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Particles/Alpha Blended"
}
