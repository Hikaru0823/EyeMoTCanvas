using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputModeSelecterUI : Singleton<InputModeSelecterUI>
{
    public Animator Animator;
    [SerializeField] private InputModeOption[] inputModeOptions;
    [SerializeField] private Image previewIcon;
    [SerializeField] private OptionTab optionTab;

    void Start()
    {
        foreach (var option in inputModeOptions)
        {
            // 各ボタンのonClickリスナーを設定
            option.Button.onClick.AddListener(() => {
                OnInputModeOptionSelected(option);
            });
        }
    }

    void OnInputModeOptionSelected(InputModeOption option)
    {
        foreach (var opt in inputModeOptions)
        {
            // ハイライトの表示・非表示を切り替え
            if (opt.HighlightImage != null)
            {
                opt.HighlightImage.enabled = (opt == option);
            }
        }
        previewIcon.sprite = option.Icon.sprite;
        GpuPainter.Instance.SetInputMode(option.InputMode);
        optionTab.CloseAllTabs();
    }


#if UNITY_EDITOR
    /// <summary>
    /// インスペクターで値が変更された時に呼ばれる（エディタのみ）
    /// </summary>
    void OnValidate()
    {
        if (inputModeOptions != null)
        {
            foreach (var option in inputModeOptions)
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
public class InputModeOption
{
    public Button Button;
    public Image Icon;
    public Image HighlightImage;
    public InputMode InputMode;


    /// <summary>
    /// ゲームオブジェクトからImageコンポーネントを自動取得
    /// </summary>
    private void AutoAssign()
    {
        if (Button != null)
        {
            Icon = Button.transform.Find("PreviewColor")?.GetComponent<Image>();;
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
