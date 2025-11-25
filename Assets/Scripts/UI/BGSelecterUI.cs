using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BGSelecterUI : SimpleHighlightSelecterOptionUI
{
    [SerializeField] private BGOption[] bgOptions;
    [SerializeField] private RawImage bgImage;

    void Start()
    {
        Init(bgOptions);
    }

    public override void OnOptionSelected(OptionButtonResources option)
    {
        base.OnOptionSelected(option);
        Screen.fullScreen = true;
        #if !UNITY_WEBGL
        TransparentWindow.Instance.SetTransparent((option as BGOption).isTransparent);
        #endif
        bgImage.color = (option as BGOption).Color;
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// インスペクターで値が変更された時に呼ばれる（エディタのみ）
    /// </summary>
    void OnValidate()
    {
        AutoAssign(bgOptions);
    }
    #endif
}

[Serializable]
public class BGOption : OptionButtonResources
{
    public Color Color;
    public bool isTransparent;
}