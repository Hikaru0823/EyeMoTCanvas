using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SimpleSelecterOptionUI : MonoBehaviour
{
    [SerializeField] private string[] optionLabels;
    [SerializeField] private TextMeshProUGUI optionText;
    [SerializeField] private UnityEvent<int> onValueChanged;
    [SerializeField] private int currentValue;

    private void Start()
    {
        onValueChanged.Invoke(currentValue);
        UpdateOptionText();
    }

    public void OnValueChanged(int value)
    {
        var newValue = Mathf.Clamp(currentValue + value, 0, optionLabels.Length - 1);
        if (newValue == currentValue) return;
        currentValue = newValue;
        onValueChanged.Invoke(currentValue);
        UpdateOptionText();
    }

    private void UpdateOptionText()
    {
        optionText.text = optionLabels[currentValue];
    }
}
