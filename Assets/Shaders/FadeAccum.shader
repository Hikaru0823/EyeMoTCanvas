Shader "Hidden/FadeAccum"
{
    Properties
    {
        _MainTex    ("Accum (Σrgb, Σw)", 2D) = "black" {}
        _FadeFactor ("Fade Factor Per Frame", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always
        Blend One Zero   // 完全上書き

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float  _FadeFactor;

            struct app
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv  : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert (app v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 acc = tex2D(_MainTex, i.uv);

                acc.rgb *= _FadeFactor;
                acc.a   *= _FadeFactor;

                return acc;
            }
            ENDCG
        }
    }
}
