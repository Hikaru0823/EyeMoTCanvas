using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SizeOptionUI : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image previewImage;
    [SerializeField] private UnityEvent<float> onValueChanged;
    [ReadOnly,SerializeField] private float minValue = 0.01f;
    [ReadOnly, SerializeField] private float maxValue = 0.04f;
    [ReadOnly,SerializeField] private float currentValue;

    private void Start()
    {
        // 初期値を設定
        var initialValue = Mathf.InverseLerp(minValue, maxValue, currentValue);
        slider.value = initialValue;
        UpdatePreviewImage();
    }

    public void OnValueChanged(float value)
    {
        // valueは0〜1の範囲で、0の時minValue、1の時maxValueになるよう線形補間
        var newValue = Mathf.Lerp(minValue, maxValue, value);
        currentValue = newValue;
        onValueChanged.Invoke(currentValue);
        UpdatePreviewImage();
    }

    private void UpdatePreviewImage()
    {
        if (previewImage != null)
        {
            Vector2 sizeDelta = previewImage.rectTransform.sizeDelta;
            sizeDelta.x = currentValue * 4000f; // currentValueを使用してプレビューサイズを調整
            sizeDelta.y = currentValue * 4000f; // 縦横比を保つため
            previewImage.rectTransform.sizeDelta = sizeDelta;
        }
    }
}
