using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Base
{
    public BrushType Type = BrushType.Pen;
    public Material Material;
    [Tooltip("オリジナル→Zero")]
    public UnityEngine.Rendering.BlendMode BlendMode = UnityEngine.Rendering.BlendMode.Zero;
    [Tooltip("ブラシのサイズ(半径)")]
    public ClampedValue<float> Radius = new ClampedValue<float>(0.02f, 0.001f, 0.1f);
    public Color Color = Color.black;
    public ClampedValue<float> Opacity = new ClampedValue<float>(0.1f, 0f, 0.1f);
    [Tooltip("エッジのぼかし(0:鋭利、1:ぼかし最大)")]
    public ClampedValue<float> EdgeFeather = new ClampedValue<float>(0.4f, 0f, 1f);
    [Tooltip("中心の重み指数（大きいほど中心が濃い）")]
    public ClampedValue<float> FalloffPow = new ClampedValue<float>(2f, 1f, 5f);
}

[System.Serializable]
public class SpeedVariation
{
    public bool Enabled = true;
    [Tooltip("感度（大きいほど変化が急）")]
    public ClampedValue<float> Sensitivity = new ClampedValue<float>(0.5f, 0.1f, 1f);
    public ClampedValue<float> MinVariation = new ClampedValue<float>(0.4f, 0f, 1.0f);
    public ClampedValue<float> MaxVariation = new ClampedValue<float>(0.4f, 0f, 1.0f);
}

[System.Serializable]
public class LineInterpolation
{
    public bool Enabled = true;
    [Tooltip("補間を開始する最大距離")]
    public ClampedValue<float> MaxDistance = new ClampedValue<float>(0.009f, 0.001f, 0.01f);
    [Tooltip("最大補間点数）")]
    public ClampedValue<int> MaxSteps = new ClampedValue<int>(18, 1, 90);
}

[System.Serializable]
public class FadeAccumulation
{
    public bool Enabled = false;
    public Material Material;
    [Tooltip("薄くなる時間（秒）")]
    public ClampedValue<float> Time = new ClampedValue<float>(5f, 0.1f, 20f);
}

[System.Serializable]
public class InkDrip
{
    public bool Enabled = false;
    public Material Material;
    [Tooltip("どれくらい塗り重ねたら垂れ始めるか")]
    public ClampedValue<float> Threshold = new ClampedValue<float>(50f, 1f, 100f);
    [Tooltip("1フレームでどれだけ流すか")]
    public ClampedValue<float> Amount = new ClampedValue<float>(0.1f, 0, 1);
}

[System.Serializable]
public class Rainbow
{
    public RainbowType Type = RainbowType.None;
    public ClampedValue<float> Speed = new ClampedValue<float>(0.1f, 0.1f, 5f);
}

[System.Serializable]
public class Spray
{
    [Tooltip("密度")]
    public ClampedValue<float> sprayDensity = new ClampedValue<float>(0.4f, 0f, 1f);
    [Tooltip("粒子サイズ")]
    public ClampedValue<float> sprayGrainScale = new ClampedValue<float>(700f, 400f, 800f);
    [Tooltip("ストローク速度")]
    public float StrokeSpeed;
}

[System.Serializable]
public class BrushSettings
{
    [Header("基本設定")]
    public Base Base;
    [Header("速度変化設定")]
    public SpeedVariation SpeedVariation;
    [Header("線分補間設定")]
    public LineInterpolation LineInterpolation;
    [Header("蓄積薄化設定")]
    public FadeAccumulation FadeAccumulation;
    [Header("ドリップ設定")]
    public InkDrip InkDrip;
    [Header("虹色設定")]
    public Rainbow Rainbow;
    [Header("スプレー設定")]
    public Spray Spray;

    public enum ModifiableProperty
    {
        Opacity,
        SpeedVariation_Enabled,
        SpeedVariation_Min,
        SpeedVariation_Max,
        SpeedVariation_Sensitivity,
        FadeAccumulation_Enabled,
        FadeAccumulation_Time,
        InkDrip_Enabled,
        InkDrip_Threshold,
        InkDrip_Amount
    }
    
    public BrushSettings()
    {
        Base = new Base();
        SpeedVariation = new SpeedVariation();
        LineInterpolation = new LineInterpolation();
        FadeAccumulation = new FadeAccumulation();
        InkDrip = new InkDrip();
        Rainbow = new Rainbow();
        Spray = new Spray();
    }

    #region 設定項目

    public void SetValue<T>(ModifiableProperty property, T value)
    {
        switch (property)
        {
            case ModifiableProperty.Opacity:
                if (value is float opacityValue)
                    Base.Opacity.Value = opacityValue;
                break;
            case ModifiableProperty.SpeedVariation_Enabled:
                if (value is bool enabled)
                    SpeedVariation.Enabled = enabled;
                break;
            case ModifiableProperty.SpeedVariation_Min:
                if (value is float minValue)
                    SpeedVariation.MinVariation.Value = minValue;
                break;
            case ModifiableProperty.SpeedVariation_Max:
                if (value is float maxValue)
                    SpeedVariation.MaxVariation.Value = maxValue;
                break;
            case ModifiableProperty.SpeedVariation_Sensitivity:
                if (value is float sensitivityValue)
                    SpeedVariation.Sensitivity.Value = sensitivityValue;
                break;
            case ModifiableProperty.FadeAccumulation_Enabled:
                if (value is bool fadeEnabled)
                    FadeAccumulation.Enabled = fadeEnabled;
                break;
            case ModifiableProperty.FadeAccumulation_Time:
                if (value is float fadeTime)
                    FadeAccumulation.Time.Value = fadeTime;
                break;
            case ModifiableProperty.InkDrip_Enabled:
                if (value is bool dripEnabled)
                    InkDrip.Enabled = dripEnabled;
                break;
            case ModifiableProperty.InkDrip_Threshold:
                if (value is float threshold)
                    InkDrip.Threshold.Value = threshold;
                break;
            case ModifiableProperty.InkDrip_Amount:
                if (value is float amount)
                    InkDrip.Amount.Value = amount;
                break;
        }
    }

    public T GetValue<T>(ModifiableProperty property)
    {
        object result = null;
        
        switch (property)
        {
            case ModifiableProperty.Opacity:
                result = Base.Opacity;
                break;
            case ModifiableProperty.SpeedVariation_Enabled:
                result = SpeedVariation.Enabled;
                break;
            case ModifiableProperty.SpeedVariation_Min:
                result = SpeedVariation.MinVariation;
                break;
            case ModifiableProperty.SpeedVariation_Max:
                result = SpeedVariation.MaxVariation;
                break;
            case ModifiableProperty.SpeedVariation_Sensitivity:
                result = SpeedVariation.Sensitivity;
                break;
            case ModifiableProperty.FadeAccumulation_Enabled:
                result = FadeAccumulation.Enabled;
                break;
            case ModifiableProperty.FadeAccumulation_Time:
                result = FadeAccumulation.Time;
                break;
            case ModifiableProperty.InkDrip_Enabled:
                result = InkDrip.Enabled;
                break;
            case ModifiableProperty.InkDrip_Threshold:
                result = InkDrip.Threshold;
                break;
            case ModifiableProperty.InkDrip_Amount:
                result = InkDrip.Amount;
                break;
        }

        if (result is T typedResult)
        {
            return typedResult;
        }
        
        return default(T);
    }
    # endregion
}
