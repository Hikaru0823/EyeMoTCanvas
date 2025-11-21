using KanKikuchi.AudioManager;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class GpuPainter : Singleton<GpuPainter>
{
    #region  キャッシュ
    static readonly int ID_MainTex = Shader.PropertyToID("_MainTex");
    static readonly int ID_PrevPos = Shader.PropertyToID("_PrevPos");
    static readonly int ID_CurrPos = Shader.PropertyToID("_CurrPos");
    static readonly int ID_Radius = Shader.PropertyToID("_Radius");
    static readonly int ID_Feather = Shader.PropertyToID("_Feather");
    static readonly int ID_FalloffPow = Shader.PropertyToID("_FalloffPow");
    static readonly int ID_BaseAlpha = Shader.PropertyToID("_BaseAlpha");
    static readonly int ID_BrushColor = Shader.PropertyToID("_BrushColor");
    static readonly int ID_BrushStyle = Shader.PropertyToID("_BrushStyle");
    static readonly int ID_NoiseTex = Shader.PropertyToID("_NoiseTex");
    static readonly int ID_SprayDensity = Shader.PropertyToID("_SprayDensity");
    static readonly int ID_GrainScale = Shader.PropertyToID("_GrainScale");
    static readonly int ID_Seed = Shader.PropertyToID("_Seed");
    #endregion

    protected override void OnSingletonAwake()
    {
        // 初期化処理
        Init();
    }

    protected override void OnSingletonDestroy()
    {
        // リソース解放
        if (rtA) rtA.Release();
        if (rtB) rtB.Release();
    }

    [SerializeField] private BrushPreset preset;
    public BrushSettings BrushSettings = new BrushSettings();

    [Header("UI")]
    public RawImage canvasImage;        // ここに最終RTを表示
    public RectTransform canvasRect;    // RawImage の RectTransform

    [Header("Canvas Settings")]
    public int width = 1920;
    public int height = 1080;
    public RenderTextureFormat rtFormat = RenderTextureFormat.ARGB32;
    public InputMode inputMode = InputMode.Gaze;
    const float moveEpsUV = 0.0001f;                     // 最小移動距離（UV）

    Vector2? lastUv = null;             // 前フレームの筆位置（UV）
    float lastUpdateTime = 0f;          // 前回の更新時刻
    float currentSpeed = 0f;            // 現在の移動速度

    [Header("UI Blocking")]
    [SerializeField] private bool enableUIBlocking = true;         // UI上での描画ブロックの有効/無効
    [SerializeField] private string blockedTag = "UIPanel";         // ポインターがUI上にあるか

    RenderTexture rtA, rtB;
    Texture2D sprayNoiseTex;
    bool useAasPrev = true;
    bool isSwitched = false;

    RenderTexture CreateRT()
    {
        var rt = new RenderTexture(width, height, 0, rtFormat);
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.filterMode = FilterMode.Bilinear;
        rt.useMipMap = false;
        rt.Create();
        return rt;
    }

    void ClearRT(RenderTexture rt, Color c)
    {
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, c);
        RenderTexture.active = prev;
    }

    private void Init()
    {
        if (sprayNoiseTex == null)
        {
            sprayNoiseTex = MakeNoiseTex(128);
        }
        BrushSettings = preset.DefaultSettings;
        BrushSettings.Base.Material.SetTexture("_NoiseTex", sprayNoiseTex);
        ClearCanvas();
    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearCanvas();
        }

        bool drawing = JudgeFromInputMode();
        bool isOverBlockedUI = IsPointerOverBlockedUI(); // UI上かどうかをチェック

        if(BrushSettings.FadeAccumulation.Enabled) BlitfadeAccum();

        if (TryGetPointerUV(out var uv))
        {
            // UI上では描画しない
            if (drawing && !isOverBlockedUI)
            {
                if (lastUv.HasValue)
                {
                    if ((uv - lastUv.Value).sqrMagnitude >= moveEpsUV * moveEpsUV)
                    {
                        if (!SEManager.Instance.IsPlaying())
                        {
                            switch (BrushSettings.Base.Type)
                            {
                                case BrushType.Pen:
                                    SEManager.Instance.Play(SEPath.PAINT_SE_01);
                                    break;
                                case BrushType.Spray:
                                    SEManager.Instance.Play(SEPath.SPRAY_NOMALIZE);
                                    break;
                                case BrushType.Penki:
                                    SEManager.Instance.Play(SEPath.PENKI_SOUND);
                                    break;
                            }
                        }
                        // 速度に基づくブラシサイズを計算
                        float deltaTime = Time.time - lastUpdateTime;
                        float dynamicBrushSize = GetSpeedBasedBrushSize(uv, lastUv.Value, deltaTime);

                        // 補間を使用して滑らかな線を描画
                        DrawInterpolatedLine(lastUv.Value, uv, dynamicBrushSize);
                        lastUv = uv;
                        lastUpdateTime = Time.time;
                    }
                }
                lastUv = uv;
            }
            else
            {
                // 描画していない、またはUI上にいる場合はストロークを切る
                lastUv = null;
            }
        }
        else
        {
            // キャンバス外に出たらストロークを切る
            lastUv = null;
        }

        // 表示更新：最新のRTをRawImageに
        canvasImage.texture = useAasPrev ? rtB : rtA;
    }

    // 虹色を生成する（HSVを使用）
    Color GetRainbowColor(float time)
    {
        float hue = 0f;
        switch (BrushSettings.Rainbow.Type)
        {
            case RainbowType.Gradation:
                hue = (time * BrushSettings.Rainbow.Speed.Value) % 1f; // 0〜1の範囲でループ
                break;
            case RainbowType.Sepalate:
                // はっきり色が変わるやつ - 6色に分割
                int colorIndex = Mathf.FloorToInt(time * BrushSettings.Rainbow.Speed.Value * 6) % 6;
                hue = colorIndex / 6f;
                break;
            default:
                hue = 0f;
                break;
        }
        return Color.HSVToRGB(hue, 1f, 1f);
    }

    Texture2D MakeNoiseTex(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.R8, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Point;
        var cols = new Color[size * size];
        for (int i = 0; i < cols.Length; i++) cols[i] = new Color(Random.value, 0, 0);
        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }

    // 速度に基づいてブラシサイズを計算
    float GetSpeedBasedBrushSize(Vector2 currentUV, Vector2 previousUV, float deltaTime)
    {
        if (!BrushSettings.SpeedVariation.Enabled || deltaTime <= 0f)
        {
            return BrushSettings.Base.Radius.Value;
        }

        // UV座標での移動距離を計算
        float distance = Vector2.Distance(currentUV, previousUV);

        // 速度を計算（UV単位/秒）
        currentSpeed = distance / deltaTime;

        // 速度を正規化（0〜1の範囲）
        // speedSensitivityで調整可能
        float normalizedSpeed = Mathf.Clamp01(currentSpeed * BrushSettings.SpeedVariation.Sensitivity.Value);

        // 速度に基づく変動率を計算
        // 遅い（0） → +10%、早い（1） → -10%
        float variation = Mathf.Lerp(BrushSettings.SpeedVariation.MaxVariation.Value, -BrushSettings.SpeedVariation.MinVariation.Value, normalizedSpeed);

        // 最終的なブラシサイズ
        float finalBrushSize = BrushSettings.Base.Radius.Value * (1f + variation);

        return finalBrushSize;
    }

    // 長い線を補間して滑らかに描画
    void DrawInterpolatedLine(Vector2 fromUV, Vector2 toUV, float brushSize)
    {
        float distance = Vector2.Distance(fromUV, toUV);

        // 距離が短い場合は通常の描画
        if (!BrushSettings.LineInterpolation.Enabled || distance <= BrushSettings.LineInterpolation.MaxDistance.Value)
        {
            BlitCapsule(fromUV, toUV, brushSize);
            return;
        }

        // 補間点数を計算（距離に基づく）
        int steps = (int)Mathf.Min(BrushSettings.LineInterpolation.MaxSteps.Value, Mathf.CeilToInt(distance / BrushSettings.LineInterpolation.MaxDistance.Value));

        Vector2 previousPoint = fromUV;

        // 補間点を順番に描画
        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 currentPoint = Vector2.Lerp(fromUV, toUV, t);

            // 各セグメントを描画
            BlitCapsule(previousPoint, currentPoint, brushSize);

            previousPoint = currentPoint;
        }
    }

    // UI上にポインターがあるかをチェック
    bool IsPointerOverBlockedUI()
    {
        if (!enableUIBlocking) return false;

        // "Panel UI"タグの特定チェック
        Vector2 screenPos = (Input.touchCount > 0) ? Input.GetTouch(0).position : Input.mousePosition;
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject != null && result.gameObject.CompareTag(blockedTag))
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// キャンバスをクリア
    /// </summary>
    public void ClearCanvas()
    {
        // RTを再作成
        if (rtA) rtA.Release();
        if (rtB) rtB.Release();
        rtA = CreateRT();
        rtB = CreateRT();
        ClearRT(rtA, Color.clear);
        ClearRT(rtB, Color.clear);
        lastUv = null;

        // 表示更新
        canvasImage.texture = rtA; // とりあえず表示
    }

    bool JudgeFromInputMode()
    {
        switch (inputMode)
        {
            case InputMode.Gaze:
                return true;
            case InputMode.Switching:
                if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0))
                {
                    if(IsPointerOverBlockedUI()) return false; // UI上では切り替えない
                    isSwitched = !isSwitched;
                }
                return isSwitched;
            case InputMode.Press:
                return Input.GetMouseButton(0) || (Input.touchCount > 0);
            default:
                return false;
        }
    }

    bool TryGetPointerUV(out Vector2 uv)
    {
        Vector2 screenPos = (Input.touchCount > 0) ? Input.GetTouch(0).position : Input.mousePosition;

        // スクリーン座標からローカル座標への変換を試行
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, null, out var local))
        {
            
            var r = canvasRect.rect;
            float u = Mathf.InverseLerp(r.xMin, r.xMax, local.x);
            float v = Mathf.InverseLerp(r.yMin, r.yMax, local.y);
            uv = new Vector2(u, v);

            // キャンバス内かどうかの詳細判定
            bool isInsideCanvas = u > 0 && u < 1 && v > 0 && v < 1;
            return isInsideCanvas;
        }
        else
        {
            // RectTransformの変換に失敗した場合（完全に画面外）
            uv = default;
            return false;
        }
    }

    // 1回のBlitで「前点→今回点」をカプセル（線分＋丸端）として描く
    void BlitCapsule(Vector2 prevUV, Vector2 currUV, float customBrushSize = -1f)
    {
        var prev = useAasPrev ? rtA : rtB;
        var next = useAasPrev ? rtB : rtA;

        // 虹色効果が有効な場合は動的に色を変更
        Color currentColor = BrushSettings.Rainbow.Type != RainbowType.None ? GetRainbowColor(Time.time) : BrushSettings.Base.Color;

        // カスタムブラシサイズが指定されていない場合はデフォルトを使用
        float actualBrushSize = customBrushSize > 0f ? customBrushSize : BrushSettings.Base.Radius.Value;

        // マテリアルへ必要パラメータをセット
        BrushSettings.Base.Material.SetTexture(ID_MainTex, prev);
        BrushSettings.Base.Material.SetVector(ID_PrevPos, new Vector4(prevUV.x, prevUV.y, 0, 0));
        BrushSettings.Base.Material.SetVector(ID_CurrPos, new Vector4(currUV.x, currUV.y, 0, 0));
        BrushSettings.Base.Material.SetFloat(ID_Radius, actualBrushSize);
        BrushSettings.Base.Material.SetFloat(ID_Feather, BrushSettings.Base.EdgeFeather.Value);   // 0.3〜0.5 推奨
        BrushSettings.Base.Material.SetFloat(ID_FalloffPow, BrushSettings.Base.FalloffPow.Value);   // 1.3〜2.2 あたり
        BrushSettings.Base.Material.SetFloat(ID_BaseAlpha, BrushSettings.Base.Opacity.Value);    // 0.4〜0.7 くらい
        BrushSettings.Base.Material.SetColor(ID_BrushColor, currentColor); // 虹色またはブラシ色
        BrushSettings.Base.Material.SetFloat(ID_BrushStyle, (float)BrushSettings.Base.Type);
        BrushSettings.Base.Material.SetTexture(ID_NoiseTex, sprayNoiseTex); // 128x128くらいのブルーノイズ
        BrushSettings.Base.Material.SetFloat(ID_SprayDensity, BrushSettings.Spray.sprayDensity.Value);
        BrushSettings.Base.Material.SetFloat(ID_GrainScale, BrushSettings.Spray.sprayGrainScale.Value);
        BrushSettings.Base.Material.SetFloat(ID_Seed, BrushSettings.Spray.StrokeSpeed);

        // 合成
        Graphics.Blit(prev, next, BrushSettings.Base.Material);

        // ピンポン
        useAasPrev = !useAasPrev;
    }

    void BlitfadeAccum()
    {
        var prev = useAasPrev ? rtA : rtB;
        var next = useAasPrev ? rtB : rtA;

        float fadeFactor = Mathf.Exp(-Mathf.Log(100f) * Time.deltaTime / BrushSettings.FadeAccumulation.Time.Value);

        BrushSettings.FadeAccumulation.Material.SetFloat("_FadeFactor", fadeFactor);

        // 合成
        Graphics.Blit(prev, next, BrushSettings.FadeAccumulation.Material);
        // ピンポン
        useAasPrev = !useAasPrev;
    }

    #region Option UI連携用

    public void SetPenSize(float sizeUV)
    {
        BrushSettings.Base.Radius.Set(sizeUV);
    }

    public void SetBrushType(BrushType type)
    {
        BrushSettings.Base.Type = type;
    }

    public void SetSpeedSensitivity(float sensitivity)
    {
        BrushSettings.SpeedVariation.Sensitivity.Set(sensitivity);
    }

    public void SetEnableSpeedVariation(bool enable)
    {
        BrushSettings.SpeedVariation.Enabled = enable;
    }

    public void SetSpeedVariation(float variation)
    {
        BrushSettings.SpeedVariation.MinVariation.Set(variation);
        BrushSettings.SpeedVariation.MaxVariation.Set(variation);
    }

    public void SetWindowSize(int idx)
    {
        switch (idx)
        {
            case 0:
                width = 1280;
                height = 720;
                break;
            case 1:
                width = 1920;
                height = 1080;
                break;
            case 2:
                width = 2560;
                height = 1440;
                break;
            case 3:
                width = 3840;
                height = 2160;
                break;
        }

        ClearCanvas();
    }

    public void SetColor(Color color, RainbowType rainbow)
    {
        Debug.Log($"SetColor called with color: {color}, rainbow: {rainbow}");
        BrushSettings.Base.Color = color;
        BrushSettings.Rainbow.Type = rainbow;
    }

    public void SetOpacity(float opacity)
    {
        BrushSettings.Base.Opacity.Set(opacity);
    }

    public void SetInputMode(InputMode mode)
    {
        inputMode = mode;
    }
    #endregion
}

public enum InputMode
{
    Gaze,
    Switching,
    Press,
}
