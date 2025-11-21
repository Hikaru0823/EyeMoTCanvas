Shader "UI/RawImagePremultiplied"
{
    Properties{ _MainTex("Accum",2D)="black"{} _Color("Tint",Color)=(1,1,1,1) }
    SubShader{
        Tags{ "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Pass{
            ZWrite Off
            ZTest  Always
            Cull   Off
            // premultiplied 表示：rgb は正規化後に α を掛ける
            Blend One OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _Color;
            struct app{ float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f{ float2 uv:TEXCOORD0; float4 pos:SV_POSITION; };
            v2f vert(app v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
            fixed4 frag(v2f i):SV_Target{
                float4 acc = tex2D(_MainTex, i.uv);   // (Σrgb, Σw)
                float  w   = acc.a;
                float3 mean = (w > 1e-6) ? acc.rgb / w : 0;  // 加重平均色
                float  alpha = saturate(w);                  // 被覆度（お好みで 1- exp(-w*k) なども可）
                float3 premul = mean * alpha;               // premulで表示
                return float4(premul, alpha) * _Color;
            }
            ENDCG
        }
    }
}
