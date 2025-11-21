using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorParetUI : Singleton<ColorParetUI>
{
    public Animator Animator;
    [SerializeField] private ColorOption[] colorOptions;
    public Sprite[] rainbowTypeSprites; // 0-gradation, 1-sepalate

    void Start()
    {
        foreach (var option in colorOptions)
        {
            // 各ボタンのonClickリスナーを設定
            option.Button.onClick.AddListener(() => {
                OnColorOptionSelected(option);
            });
        }
    }

    void OnColorOptionSelected(ColorOption option)
    {
        foreach (var opt in colorOptions)
        {
            // ハイライトの表示・非表示を切り替え
            if (opt.HighlightImage != null)
            {
                opt.HighlightImage.enabled = (opt == option);
            }
        }
        // 選択された色を適用
        GpuPainter.Instance.SetColor(option.Color, option.RainbowType);
        BrushSelecterUI.Instance.ChangeTipColor(option.Color, option.RainbowType);
    }


#if UNITY_EDITOR
    /// <summary>
    /// インスペクターで値が変更された時に呼ばれる（エディタのみ）
    /// </summary>
    void OnValidate()
    {
        if (colorOptions != null)
        {
            foreach (var option in colorOptions)
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
public class ColorOption
{
    public Button Button;
    public Color Color;
    public Image HighlightImage;
    public RainbowType RainbowType = RainbowType.None;


    /// <summary>
    /// ゲームオブジェクトからImageコンポーネントを自動取得
    /// </summary>
    private void AutoAssign()
    {
        if (Button != null)
        {
            Color = Button.transform.Find("PreviewColor")?.GetComponent<Image>()?.color ?? Color.white;
            HighlightImage = Button.transform.Find("Highlight")?.GetComponent<Image>();
        }
        else
        {
            HighlightImage = null;
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

public enum RainbowType
{
    None,
    Gradation,
    Sepalate
}
