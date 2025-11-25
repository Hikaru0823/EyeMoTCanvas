Shader "Unlit/InkDrip"
{
    Properties
    {
        _MainTex      ("Accum (Σrgb, Σw)", 2D) = "black" {}
        _DripThreshold("Drip Threshold", Float) = 50.0
        _DripAmount   ("Drip Amount", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Pass
        {
            ZWrite Off
            ZTest  Always
            Cull   Off
            Blend  One Zero

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _DripThreshold;
            float _DripAmount;

            struct app {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f {
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
                float2 uv = i.uv;

                // 自分のピクセル
                float4 self = tex2D(_MainTex, uv);

                // 「上のピクセル」からインクが落ちてくるイメージ
                float2 uvUp = uv + float2(0.0, _MainTex_TexelSize.y);
                float4 up   = tex2D(_MainTex, uvUp);

                float selfW = self.a;
                float upW   = up.a;

                float th    = _DripThreshold;
                float range = max(th * 0.25, 1e-4);

                // 自分から「下に流れ出す」量
                float selfThick = saturate((selfW - th) / range);
                float selfOut   = selfThick * _DripAmount;

                // 上から「流れ込んでくる」量
                float upThick = saturate((upW - th) / range);
                float inFlow  = upThick * _DripAmount;

                float3 rgb = self.rgb * (1.0 - selfOut) + up.rgb * inFlow;
                float  w   = self.a   * (1.0 - selfOut) + up.a   * inFlow;

                return float4(rgb, w);
            }
            ENDCG
        }
    }
}
