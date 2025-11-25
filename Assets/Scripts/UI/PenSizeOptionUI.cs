using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PenSizeOptionUI : SimpleHighlightSelecterOptionUI
{
    [SerializeField] private Image previewIcon;
    [SerializeField] PensizeOption[] pensizeOptions;
    [SerializeField] private OptionTab optionTab;

    void Start()
    {
        Init(pensizeOptions);
    }

    public override void OnOptionSelected(OptionButtonResources option)
    {
        base.OnOptionSelected(option);
        previewIcon.transform.localScale = option.PreviewImage.transform.localScale;
        GpuPainter.Instance.SetPenSize((option as PensizeOption).Size);
        optionTab.CloseAllTabs();
    }

    #if UNITY_EDITOR
    /// <summary>
    /// インスペクターで値が変更された時に呼ばれる（エディタのみ）
    /// </summary>
    void OnValidate()
    {
        AutoAssign(pensizeOptions);
    }
    #endif
}

[Serializable]
public class PensizeOption : OptionButtonResources
{
    public float Size;
}
