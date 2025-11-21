using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SimpleToggleOptionUI : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private BrushSettings.ModifiableProperty optionType;
    [SerializeField] private UnityEvent<bool> onValueChanged;
    [ReadOnly, SerializeField] private bool currentValue;

    private void Start()
    {
        // 初期値を設定
        currentValue = GpuPainter.Instance.BrushSettings.GetValue<bool>(optionType);
        toggle.isOn = currentValue;
    }

    public void OnValueChanged(bool value)
    {
        currentValue = value;
        GpuPainter.Instance.BrushSettings.SetValue(optionType, currentValue);
        onValueChanged.Invoke(!currentValue);
    }
}
