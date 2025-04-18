using UnityEngine;
using System.Collections;

public class SkyboxAnimator : MonoBehaviour
{
    public float pulseSpeed = 1f;
    public Color[] beatColors;
    private int currentColorIndex = 0;
    private Material skyboxMaterial;

    private void Start()
    {
        skyboxMaterial = RenderSettings.skybox;
        BeatManager.OnBeatChange += OnBeat;
    }

    private void OnBeat()
    {
        // Example: Change tint color each beat
        if (beatColors.Length > 0)
        {
            currentColorIndex = (currentColorIndex + 1) % beatColors.Length;
            skyboxMaterial.SetColor("_Tint", beatColors[currentColorIndex]);
        }

        // Example: Animate exposure (pulse)
        StartCoroutine(PulseExposure());
    }

    private IEnumerator PulseExposure()
    {
        float t = 0;
        float start = 1f;
        float end = 1.5f;
        while (t < 1)
        {
            t += Time.deltaTime * pulseSpeed;
            float value = Mathf.Lerp(start, end, Mathf.Sin(t * Mathf.PI)); // pulse-y
            skyboxMaterial.SetFloat("_Exposure", value);
            yield return null;
        }
        skyboxMaterial.SetFloat("_Exposure", start); // reset
    }

    private void OnDestroy()
    {
        BeatManager.OnBeatChange -= OnBeat;
    }
}
