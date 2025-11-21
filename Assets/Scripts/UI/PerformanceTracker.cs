using TMPro;
using UnityEngine;

public class PerformanceTracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private TextMeshProUGUI memoryText;

    [Header("Performance Settings")]
    public bool enableMonitoring = true;
    public float updateInterval = 1f;
    
    
    private float timer;
    private int frameCount;
    private float deltaTimeSum;
    
    void Update()
    {
        if (!enableMonitoring) return;
        
        frameCount++;
        deltaTimeSum += Time.unscaledDeltaTime;
        timer += Time.unscaledDeltaTime;
        
        if (timer >= updateInterval)
        {
            CheckPerformance();
            ResetCounters();
        }
    }
    
    void CheckPerformance()
    {
        float avgFPS = frameCount / deltaTimeSum;
        long memoryMB = System.GC.GetTotalMemory(false) / 1024 / 1024;

        fpsText.text = $"{avgFPS:F1}";
        memoryText.text = $"{memoryMB}MB";
    }
    
    void ResetCounters()
    {
        timer = 0f;
        frameCount = 0;
        deltaTimeSum = 0f;
    }
}