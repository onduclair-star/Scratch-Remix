using TMPro;
using UnityEngine;

public class FpsDisplayer : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    public float smoothing = 0.1f;

    private float smoothedFPS = 120f;

    void Update()
    {
        float currentFPS = 1f / Time.unscaledDeltaTime;
        smoothedFPS = Mathf.Lerp(smoothedFPS, currentFPS,
            1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime));

        fpsText.text = $"FPS(SMO): {smoothedFPS:F0}";
    }
}
