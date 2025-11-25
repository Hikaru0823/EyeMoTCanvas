using UnityEngine;

public class ResourcesManager : Singleton<ResourcesManager>
{
    public static readonly string Panel_In = "Panel In";
    public static readonly string Panel_Out = "Panel Out";
    public Sprite[] rainbowTypeSprites; // 0-gradation, 1-sepalate
}
