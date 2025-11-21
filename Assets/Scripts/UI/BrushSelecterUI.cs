using System;
using KanKikuchi.AudioManager;
using UnityEngine;
using UnityEngine.UI;

public class BrushSelecterUI : Singleton<BrushSelecterUI>
{
    public Animator Animator;
    [SerializeField] private BrushOption[] brushOptions;

    void Start()
    {
        foreach (var option in brushOptions)
        {
            // 各ボタンのonClickリスナーを設定
            option.Button.onClick.AddListener(() =>
            {
                OnBrushOptionSelected(option);
            });
        }
        ChangeTipColor(GpuPainter.Instance.BrushSettings.Base.Color, GpuPainter.Instance.BrushSettings.Rainbow.Type);
    }
    
    public void ChangeTipColor(Color color, RainbowType rainbowType = RainbowType.None)
    {
        foreach (var option in brushOptions)
        {
            if (option.TipImage != null)
            {
                option.TipImage.color = color;
                var tipImage = option.TipImage.transform.Find("TipImage")?.GetComponent<Image>();
                switch (rainbowType)
                {
                    case RainbowType.None:
                        tipImage.enabled = false;
                        break;
                    case RainbowType.Gradation:
                        tipImage.enabled = true;
                        tipImage.sprite = ColorParetUI.Instance.rainbowTypeSprites[0];
                        break;
                    case RainbowType.Sepalate:
                        tipImage.enabled = true;
                        tipImage.sprite = ColorParetUI.Instance.rainbowTypeSprites[1];
                        break;
                }
            }
        }
    }

    void OnBrushOptionSelected(BrushOption option)
    {
        foreach (var opt in brushOptions)
        {
            if (opt == option)
            {
                if (!option.BrushUI.ButtonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Selected"))
                    option.BrushUI.ButtonAnimator.Play("Selected");
                continue;
            }
            if (opt.BrushUI.ButtonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Selected"))
                opt.BrushUI.ButtonAnimator.Play("Disabled");
        }
        
        switch (option.Type)
        {
            case BrushType.Pen:
                SEManager.Instance.Play(SEPath.PEN_SELECTED);
                break;
            case BrushType.Spray:
                SEManager.Instance.Play(SEPath.SPLAY_SELECTED);
                break;
        }
        // 選択されたブラシタイプ を適用
        GpuPainter.Instance.SetBrushType(option.Type);
    }

    #if UNITY_EDITOR
    /// <summary>
    /// インスペクターで値が変更された時に呼ばれる（エディタのみ）
    /// </summary>
    void OnValidate()
    {
        if (brushOptions != null)
        {
            foreach (var option in brushOptions)
            {
                if (option != null)
                {
                    // 各ColorOptionのImageコンポーネント自動割り当てを実行
                    option.ValidateComponents();
                }
            }
        }
    }
#endif
}

[Serializable]
public class BrushOption
{
    public Button Button;
    public BrushType Type;
    public Image TipImage;
    public BrushUI BrushUI;

        /// <summary>
    /// ゲームオブジェクトからImageコンポーネントを自動取得
    /// </summary>
    private void AutoAssign()
    {
        if (Button != null)
        {
            TipImage = Button.transform.Find("Tip")?.GetComponent<Image>();
            BrushUI = Button.GetComponent<BrushUI>();
        }
        else
        {
            TipImage = null;
            BrushUI = null;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// インスペクターでの変更を検証してコンポーネントを自動割り当て（エディタのみ）
    /// </summary>
    public void ValidateComponents()
    {
        AutoAssign();
    }
#endif
}


public enum BrushType
{
    Pen,
    Spray,
    Penki,
    Eraser
}