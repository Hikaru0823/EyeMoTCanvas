using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SizeBySpeedOptionUI : MonoBehaviour
{
    [SerializeField] private Slider slider_sensitivity, slider_variation;
    [SerializeField] private TextMeshProUGUI text_sensitivity, text_variation;
    [SerializeField] private Toggle toggle_enable;
    [SerializeField] private Image hidePanel;
    [SerializeField] private UnityEvent<float> onValueChanged_sensitivity, onValueChanged_variation;
    [SerializeField] private UnityEvent<bool> onValueChanged_enable;
    [SerializeField] private float minValue_sensitivity = 0.1f, maxValue_sensitivity = 1f;
    [SerializeField] private float minValue_variation = 0.1f, maxValue_variation = 0.5f;
    [SerializeField] private float currentValue_sensitivity = 1f, currentValue_variation = 0.4f;
    [SerializeField] private bool isEnabled = true;

    private void Start()
    {
        // 初期値を設定
        var initialValue_sensitivity = Mathf.InverseLerp(minValue_sensitivity, maxValue_sensitivity, currentValue_sensitivity);
        var initialValue_variation = Mathf.InverseLerp(minValue_variation, maxValue_variation, currentValue_variation);
        slider_sensitivity.value = initialValue_sensitivity;
        slider_variation.value = initialValue_variation;
        OnToggleChanged(isEnabled);
    }

    public void OnValueChanged_Sensitivity(float value)
    {
        // valueは0〜1の範囲で、0の時minValue、1の時maxValueになるよう線形補間
        var newValue = Mathf.Lerp(minValue_sensitivity, maxValue_sensitivity, value);
        currentValue_sensitivity = newValue;
        text_sensitivity.text = currentValue_sensitivity.ToString("F1");
        onValueChanged_sensitivity.Invoke(currentValue_sensitivity);
    }

    public void OnValueChanged_Variation(float value)
    {
        // valueは0〜1の範囲で、0の時minValue、1の時maxValueになるよう線形補間
        var newValue = Mathf.Lerp(minValue_variation, maxValue_variation, value);
        currentValue_variation = newValue;
        text_variation.text = ((int)(currentValue_variation * 100)).ToString("F0");
        onValueChanged_variation.Invoke(currentValue_variation);
    }

    public void OnToggleChanged(bool isOn)
    {
        isEnabled = isOn;
        hidePanel.gameObject.SetActive(!isOn);
        onValueChanged_enable.Invoke(isEnabled);
    }
}
