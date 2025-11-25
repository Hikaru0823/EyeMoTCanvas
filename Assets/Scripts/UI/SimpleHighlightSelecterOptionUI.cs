using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SimpleHighlightSelecterOptionUI : MonoBehaviour
{
    private OptionButtonResources[] options;

    public void Init(OptionButtonResources[] _options)
    {
        options = _options;
        foreach (var option in options)
        {
            // 各ボタンのonClickリスナーを設定
            option.Button.onClick.AddListener(() => {
                OnOptionSelected(option);
            });
        }
    }

    public virtual void OnOptionSelected(OptionButtonResources option)
    {
        foreach (var opt in options)
        {
            // ハイライトの表示・非表示を切り替え
            if (opt.HighlightImage != null)
            {
                opt.HighlightImage.enabled = (opt == option);
            }
        }
    }


#if UNITY_EDITOR
    /// <summary>
    /// インスペクターで値が変更された時に呼ばれる（エディタのみ）
    /// </summary>
    public void AutoAssign(OptionButtonResources[] _options)
    {
        options = _options;
        if (options != null)
        {
            foreach (var option in options)
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
public class OptionButtonResources
{
    public Button Button;
    public Image PreviewImage;
    public Image HighlightImage;

    /// <summary>
    /// ゲームオブジェクトから各コンポーネントを自動取得
    /// </summary>
    private void AutoAssign()
    {
        if (Button != null)
        {
            PreviewImage = Button.transform.Find("PreviewImage")?.GetComponent<Image>();
            HighlightImage = Button.transform.Find("Highlight")?.GetComponent<Image>();
        }
        else
        {
            PreviewImage = null;
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
