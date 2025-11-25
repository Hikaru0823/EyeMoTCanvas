using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputModeSelecterUI : SimpleHighlightSelecterOptionUI
{
    [SerializeField] private Image previewIcon;
    [SerializeField] private OptionTab optionTab;
    [SerializeField] private InputModeOption[] inputModeOptions;

    void Start()
    {
        Init(inputModeOptions);
    }

    public override void OnOptionSelected(OptionButtonResources option)
    {
        base.OnOptionSelected(option);
        previewIcon.sprite = option.PreviewImage.sprite;
        GpuPainter.Instance.SetInputMode((option as InputModeOption).InputMode);
        optionTab.CloseAllTabs();
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// インスペクターで値が変更された時に呼ばれる（エディタのみ）
    /// </summary>
    void OnValidate()
    {
        AutoAssign(inputModeOptions);
    }
    #endif
}

[Serializable]
public class InputModeOption : OptionButtonResources
{
    public InputMode InputMode;
}