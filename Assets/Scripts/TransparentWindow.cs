using System;
using UnityEngine;
using UnityEngine.UI;

#if !UNITY_WEBGL
using Kirurobo;

public class TransparentWindow : Singleton<TransparentWindow>
{
    [SerializeField] Button transparentButton;

    void Start()
    {
        #if UNITY_WEBGL
        transparentButton.interactable = false;
        #endif
    }

    public void SetTransparent(bool isTransparent)
    {
        UniWindowController.current.isTransparent = isTransparent;
    }
}
#endif