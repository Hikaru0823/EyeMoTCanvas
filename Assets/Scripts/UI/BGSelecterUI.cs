using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BGSelecterUI : Singleton<BGSelecterUI>
{
    public Animator Animator;
    [SerializeField] private BGOption[] bgOptions;
    [SerializeField] private RawImage bgImage;

    void Start()
    {
        foreach (var option in bgOptions)
        {
            // 各ボタンのonClickリスナーを設定
            option.Button.onClick.AddListener(() => {
                OnBGOptionSelected(option);
            });
        }
    }

    void OnBGOptionSelected(BGOption option)
    {
        foreach (var opt in bgOptions)
        {
            // ハイライトの表示・非表示を切り替え
            if (opt.HighlightImage != null)
            {
                opt.HighlightImage.enabled = (opt == option);
            }
        }

        Screen.fullScreen = true;
        #if !UNITY_WEBGL
        TransparentWindow.Instance.SetTransparent(option.isTransparent);
        #endif
        bgImage.color = option.Color;
    }


#if UNITY_EDITOR
    /// <summary>
    /// インスペクターで値が変更された時に呼ばれる（エディタのみ）
    /// </summary>
    void OnValidate()
    {
        if (bgOptions != null)
        {
            foreach (var option in bgOptions)
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
public class BGOption
{
    public Button Button;
    public Color Color;
    public Image HighlightImage;
    public bool isTransparent;


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
