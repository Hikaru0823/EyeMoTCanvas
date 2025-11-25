using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColorParetUI : SimpleHighlightSelecterOptionUI
{
    [SerializeField] private ColorOption[] colorOptions;

    void Start()
    {
        Init(colorOptions);
    }

    public override void OnOptionSelected(OptionButtonResources option)
    {
        base.OnOptionSelected(option);
        GpuPainter.Instance.SetColor((option as ColorOption).Color, (option as ColorOption).RainbowType);
        BrushSelecterUI.Instance.ChangeTipColor((option as ColorOption).Color, (option as ColorOption).RainbowType);
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// インスペクターで値が変更された時に呼ばれる（エディタのみ）
    /// </summary>
    void OnValidate()
    {
        AutoAssign(colorOptions);
    }
    #endif
}

[Serializable]
public class ColorOption : OptionButtonResources
{
    public Color Color;
    public RainbowType RainbowType = RainbowType.None;
}

public enum RainbowType
{
    None,
    Gradation,
    Sepalate
}
