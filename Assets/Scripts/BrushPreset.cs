using UnityEngine;

[CreateAssetMenu(menuName="Brush/Preset")]
public class BrushPreset : ScriptableObject
{
    public int SchemaVersion = 1;
    public BrushSettings DefaultSettings = new BrushSettings();
}
