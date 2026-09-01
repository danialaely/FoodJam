Shader "FateloomFx/TrailGlow"
{
    // Yıldız patlamalarından ve konfetilerden çıkan kuyrukluk izleri için
    Properties
    {
        _MainTex ("Trail Texture", 2D) = "white" {}
        _Color ("Trail Color", Color) = (1, 0.8, 0.2, 0.8)
        _Intensity ("Intensity", Range(0, 5)) = 2.0
        _FadeAlongTrail ("Fade Along Trail", Range(0, 3)) = 1.5
        _WidthFade ("Width Fade", Range(0, 5)) = 2.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+10"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Blend SrcAlpha One // Soft additive
        Cull Off
        ZWrite Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                UNITY_FOG_COORDS(1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Intensity;
            float _FadeAlongTrail;
            float _WidthFade;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);

                // UV.x boyunca trail sönmesi (0 = baş, 1 = kuyruk)
                float trailFade = pow(1.0 - i.uv.x, _FadeAlongTrail);

                // UV.y boyunca genişlik sönmesi (kenarlar yumuşak)
                float widthCenter = abs(i.uv.y - 0.5) * 2.0;
                float widthFade = 1.0 - pow(widthCenter, _WidthFade);

                fixed4 col = tex * _Color * i.color;
                col.rgb *= _Intensity;
                col.a *= trailFade * widthFade;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
