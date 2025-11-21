using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorOptionUI : MonoBehaviour
{
    [SerializeField] private Slider slider_R, slider_G, slider_B, slider_A;
    [SerializeField] private ButtonParams preview, rainbow;
    [SerializeField] private UnityEvent<Color, bool> onValueChanged;
    [SerializeField] private Color currentColor = Color.black;
    private bool isRainbow = false;

    private void Start()
    {
        UpdateSliders();
        UpdatePreviewImage();
        UpdateHighlightImage();
        slider_A.onValueChanged.AddListener(OnValueChanged);
        slider_R.onValueChanged.AddListener(OnValueChanged);
        slider_G.onValueChanged.AddListener(OnValueChanged);
        slider_B.onValueChanged.AddListener(OnValueChanged);
        onValueChanged.Invoke(currentColor, isRainbow);
    }

    public void OnValueChanged(float _)
    {
        currentColor = new Color(slider_R.value, slider_G.value, slider_B.value, slider_A.value);
        onValueChanged.Invoke(currentColor, isRainbow);
        UpdatePreviewImage();
    }

    public void OnButtonClick(bool isRainbowMode)
    {
        isRainbow = isRainbowMode;
        onValueChanged.Invoke(currentColor, isRainbow);
        UpdateHighlightImage();
    }

    private void UpdateSliders()
    {
        slider_R.value = currentColor.r;
        slider_G.value = currentColor.g;
        slider_B.value = currentColor.b;
        slider_A.value = currentColor.a;
    }

    private void UpdatePreviewImage()
    {
        if (preview != null && preview.targetImage != null)
        {
            preview.targetImage.color = currentColor;
        }
    }

    private void UpdateHighlightImage()
    {
        if (rainbow != null && rainbow.highlightImage != null)
        {
            preview.highlightImage.enabled = !isRainbow;
            rainbow.highlightImage.enabled = isRainbow;
        }
    }
}

[Serializable]
public class ButtonParams
{
    public Image targetImage;
    public Image highlightImage;
}
