/*
 * BrushShader - GPUベースのペイントブラシシェーダー
 * 
 * 機能概要:
 * - カプセル形状（線分+丸端）のブラシストロークを描画
 * - 前の位置から現在の位置への連続線を1回のドローコールで実現
 * - 重み付き平均による半透明合成（アルファブレンドの代替）
 * - アスペクト比補正による正確な円形ブラシ
 * - エッジフェザリングとフォールオフによる自然な筆触
 */

Shader "Unlit/BrushShader"
{
    Properties{
        // 累積テクスチャ（RGB=Σrgb, Alpha=Σw）前フレームまでの描画結果
        _BrushStyle ("Brush Style (0=Normal, 1=Spray)", Float) = 0
        _MainTex   ("Accum (Σrgb, Σw)", 2D) = "black" {}
        
        // ブラシの設定
        _BrushColor("Brush Color", Color) = (1,0,0,1)        // ブラシの色（RGB）
        _PrevPos   ("Prev Pos (UV)", Vector) = (0.5,0.5,0,0) // 前回の位置（UV座標）
        _CurrPos   ("Curr Pos (UV)", Vector) = (0.6,0.6,0,0) // 現在の位置（UV座標）
        _Radius    ("Radius (UV)", Float) = 0.03             // ブラシの半径（UV単位）

        //プレーの形状制御
        _NoiseTex   ("Noise Tex", 2D) = "white" {}
        _SprayDensity("Spray Density", Range(0,1)) = 0.6
        _GrainScale("Grain Scale", Float) = 400.0
        _Seed("Random Seed", Float) = 0.0
        
        // ブラシの形状制御
        _Feather   ("Edge Feather", Range(0,1)) = 0.35       // エッジのぼかし量（0=ハード, 1=ソフト）
        _FalloffPow("Center Heavier", Range(0.8,4)) = 1.8    // 中心の重み指数（大きいほど中心が濃い）
        _BaseAlpha ("Base Weight", Range(0,1)) = 0.6         // 基本の不透明度/重み
    }
    
    SubShader{
        // 透明オブジェクトとして扱い、通常より後にレンダリング
        Tags{ "Queue"="Transparent" "RenderType"="Transparent" }
        
        Pass{
            // 深度バッファ設定
            ZWrite Off    // 深度書き込み無効（透明オブジェクトなので）
            ZTest  Always // 深度テスト無効（常に描画）
            Cull   Off    // 面カリング無効（両面描画）
            
            // === カスタムブレンド（重み付き平均）は一時的にコメントアウト ===
            Blend One Zero  // src * 1 + dst * 0 = src（上書き）

            CGPROGRAM
            #pragma vertex vert      // 頂点シェーダー関数名
            #pragma fragment frag    // フラグメントシェーダー関数名
            #include "UnityCG.cginc" // Unity標準の関数・マクロ

            // Properties からの入力変数
            sampler2D _MainTex;           // 累積テクスチャ（固定機能ブレンドでは不使用だが互換性のため残す）
            float4 _MainTex_TexelSize;    // テクセルサイズ（x=1/width, y=1/height, z=width, w=height）
            float4 _BrushColor;           // ブラシ色
            float4 _PrevPos, _CurrPos;    // 前回・現在位置
            float  _Radius, _Feather, _FalloffPow, _BaseAlpha; // ブラシパラメータ
            float  _BrushStyle, _SprayDensity, _GrainScale, _Seed;           // ブラシスタイル（0=通常, 1=スプレー）
            sampler2D _NoiseTex;          // ノイズテクスチャ（スプレーブラシ用）

            // 頂点シェーダーの入力構造体
            struct app { 
                float4 vertex : POSITION;    // 頂点位置（オブジェクト空間）
                float2 uv     : TEXCOORD0;   // UV座標
            };
            
            // 頂点→フラグメント間のデータ構造体
            struct v2f { 
                float2 uv  : TEXCOORD0;      // UV座標
                float4 pos : SV_POSITION;    // クリップ空間位置
            };

            // 頂点シェーダー：頂点をクリップ空間に変換し、UVをそのまま渡す
            v2f vert(app v){ 
                v2f o; 
                o.pos = UnityObjectToClipPos(v.vertex); // オブジェクト→クリップ空間変換
                o.uv = v.uv; 
                return o; 
            }

            /*
             * アスペクト比補正関数
             * テクスチャの縦横比が1:1でない場合、円が楕円になるのを防ぐ
             * X座標を(height/width)倍して正方形空間にマッピング
             */
            float2 ToIso(float2 uv){
                uv.x *= _MainTex_TexelSize.y / _MainTex_TexelSize.x; // aspect ratio correction
                return uv;
            }
            
            /*
             * 点から線分への最短距離計算
             * P: 計算対象の点
             * A, B: 線分の両端点
             * 戻り値: 点Pから線分ABへの最短距離
             * 
             * アルゴリズム:
             * 1. 線分AB上で点Pに最も近い点Hを求める
             * 2. 射影係数t = dot(AP, AB) / |AB|²で線分上の位置を計算
             * 3. tを[0,1]にクランプして線分内に制限
             * 4. H = A + t*AB で最近点を計算
             * 5. |P-H|で距離を返す
             */
            float DistPointToSegment(float2 P, float2 A, float2 B){
                float2 AP = P - A;           // A→P ベクトル
                float2 AB = B - A;           // A→B ベクトル（線分）
                float t = 0;                 // 射影係数
                float d2 = dot(AB, AB);      // 線分の長さの二乗
                
                // ゼロ除算防止：線分が点の場合は点Aとの距離
                if(d2 > 1e-12) 
                    t = saturate(dot(AP, AB) / d2); // 射影係数をクランプ
                
                float2 H = A + t * AB;       // 線分上の最近点
                return length(P - H);        // 距離を返す
            }
            
            /*
             * カプセル形状マスク生成関数
             * uv: 現在のピクセル位置
             * p0, p1: カプセルの両端点（ブラシの前回・現在位置）
             * Riso: 半径（アスペクト比補正済み）
             * feather: エッジのぼかし量
             * powK: 中心重み指数
             * 戻り値: そのピクセルでのブラシ強度（0〜1）
             */
            float MaskCapsule(float2 uv, float2 p0, float2 p1, float Riso, float feather, float powK){
                // アスペクト比補正
                float2 P = ToIso(uv);   // 現在ピクセル
                float2 A = ToIso(p0);   // 開始点
                float2 B = ToIso(p1);   // 終了点
                
                // カプセル軸（線分）からの距離
                float d = DistPointToSegment(P, A, B);
                
                // アンチエイリアシング：隣接ピクセルとの距離差
                float aa = fwidth(d);
                
                // フェザリング幅：最小でもアンチエイリアシング分は確保
                float w = max(aa, Riso * feather);
                
                // 内側・外側境界
                float inner = max(0.0, Riso - w);  // 完全不透明領域
                float outer = Riso + w;            // 完全透明領域
                
                // スムーズステップでエッジをぼかす
                float m = 1.0 - smoothstep(inner, outer, d);
                
                // 中心ほど重くする（自然な筆圧効果）
                return pow(saturate(m), powK);
            }

            float MaskCapsuleHard(float2 uv, float2 p0, float2 p1, float Riso)
            {
                float2 P = ToIso(uv);
                float2 A = ToIso(p0);
                float2 B = ToIso(p1);

                float d = DistPointToSegment(P, A, B);

                // 半径以内なら完全塗りつぶし
                return step(d, Riso);   // d <= Riso → 1   /   d > Riso → 0
            }

            /*
             * フラグメントシェーダー：各ピクセルの最終色を決定
             * 
             * === 固定機能アルファブレンド版 ===
             * 標準的な透明描画：color.rgb, color.a を出力してBlend設定に任せる
             * 
             * === 重み付き平均（カスタムブレンド）版はコメントアウト ===
             * 蓄積方式:
             * RGB = Σ(color_i * weight_i)  重み付き色の累積
             * A   = Σ(weight_i)            重みの累積
             * 最終色 = RGB / A             正規化で平均色を取得
             */
            fixed4 frag(v2f i) : SV_Target{
                // === 固定機能アルファブレンド版 ===
                
                // アスペクト比補正された半径
                float Riso = _Radius * (_MainTex_TexelSize.y / _MainTex_TexelSize.x);
                
                // 現在ピクセルでのブラシマスク値（0〜1）
                float m = MaskCapsule(i.uv, _PrevPos.xy, _CurrPos.xy, Riso, _Feather, _FalloffPow);

                int style = (int)round(_BrushStyle);
                // ── スプレーブラシ：ノイズベースの点描 ──
                if (style == 1)
                {
                    // シンプルなノイズサンプリング
                    float2 noiseUV = i.uv * _GrainScale + float2(_Seed, _Seed * 0.5);
                    float noise = tex2D(_NoiseTex, noiseUV).r;

                    
                    // 中心にいくほど密度が高くなる確率計算
                    // m = 0(外周) → density = 0.1, m = 1(中心) → density = _SprayDensity
                    float densityByDistance = lerp(0.0, _SprayDensity, m);
                    float threshold = 1.0 - densityByDistance;
                    float spray = step(threshold, noise);

                    
                    // スプレー効果を適用
                    m *= spray;
                }
                
                // 前フレームまでの蓄積値を読み取り
                float4 acc = tex2D(_MainTex, i.uv);

                if (style == 2)
                {
                    m = MaskCapsuleHard(i.uv, _PrevPos.xy, _CurrPos.xy, Riso);
                    // まず過去の色をマスク範囲内で完全削除
                    acc.rgb *= (1.0 - m);
                    acc.a   *= (1.0 - m);

                    // その上に今回の色を完全塗りつぶし (w=1固定 or _BaseAlphaで調整可)
                    float3 outRgb = acc.rgb + _BrushColor.rgb * m;
                    float  outW   = acc.a   + m;

                    return float4(outRgb, outW);
                }

                // 今回の重み：ベース重み × マスク値
                float w = _BaseAlpha * m;

                // 今回の重み付き色
                float3 addRgb = _BrushColor.rgb * w;
                float  addW   = w;

                float3 outRgb = acc.rgb + addRgb;  // 重み付き色を累積
                float  outW   = acc.a   + addW;    // 重みを累積

                // 次回のために蓄積値を保存
                // 実際の表示では outRgb/outW で正規化される（RawImageのマテリアルで）
                return float4(outRgb, outW);
            }
            ENDCG
        }
    }
}
