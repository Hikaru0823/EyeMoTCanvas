using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SimpleSliderOptionUI : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private BrushSettings.ModifiableProperty optionType;
    [SerializeField] private UnityEvent<object> onValueChanged;
    [SerializeField] private string format = "F2";
    [SerializeField] private bool showValueLerp = true;
    [SerializeField] private float multiplier = 1.0f;
    [ReadOnly, SerializeField] private float minValue;
    [ReadOnly, SerializeField] private float maxValue;
    [ReadOnly, SerializeField] private float currentValue;

    private void Start()
    {
        // 初期値を設定
        var _value = GpuPainter.Instance.BrushSettings.GetValue<ClampedValue<float>>(optionType);
        minValue = _value.Min;
        maxValue = _value.Max;
        currentValue = _value.Value;
        var initialValue = Mathf.InverseLerp(minValue, maxValue, currentValue);
        //Debug.Log($"InitialValue: {initialValue}, CurrentValue: {currentValue}, Min: {minValue}, Max: {maxValue}");
        slider.value = initialValue;
        valueText.text = showValueLerp ? initialValue.ToString(format) : (currentValue * multiplier).ToString(format);
    }

    public void OnValueChanged(float value)
    {
        // valueは0〜1の範囲で、0の時minValue、1の時maxValueになるよう線形補間
        var newValue = Mathf.Lerp(minValue, maxValue, value);
        currentValue = newValue;
        GpuPainter.Instance.BrushSettings.SetValue(optionType, currentValue);
        valueText.text = showValueLerp ? value.ToString(format) : (currentValue * multiplier).ToString(format);
        //Debug.Log($"VALUE: {value}, NewValue: {newValue}, SliderValue: {value}");
        onValueChanged.Invoke(currentValue);
    }

}
