using UnityEngine;

public class RingPulse : MonoBehaviour
{
    private Vector3 originalScale;
    private float targetScaleZ = 1f;
    private float currentScaleZ = 1f;

    [SerializeField] private float scaleMultiplier = 1.5f;
    [SerializeField] private float smoothSpeed = 5f;

    [SerializeField] private Material ringMaterial;
    private Color baseEmissionColor;

    void Start()
    {
        originalScale = transform.localScale;
        baseEmissionColor = ringMaterial.GetColor("_EmissionColor");

        BeatManager.OnBeatChange += Pulse;
    }

    void OnDestroy()
    {
        BeatManager.OnBeatChange -= Pulse;
    }

    void Pulse()
    {
        targetScaleZ = scaleMultiplier;

        // Increase emission temporarily on beat
        ringMaterial.SetColor("_EmissionColor", baseEmissionColor * 5f); // glow boost
        Invoke("ResetEmission", 0.1f);
    }

    void ResetEmission()
    {
        ringMaterial.SetColor("_EmissionColor", baseEmissionColor);
    }

    void Update()
    {
        // Smoothly scale Z
        currentScaleZ = Mathf.Lerp(currentScaleZ, targetScaleZ, Time.deltaTime * smoothSpeed);
        Vector3 newScale = new Vector3(originalScale.x, originalScale.y, originalScale.z * currentScaleZ);
        transform.localScale = newScale;
    }

    void LateUpdate()
    {
        if (Mathf.Abs(currentScaleZ - targetScaleZ) < 0.01f)
        {
            targetScaleZ = 1f; // reset back to original scale
        }
    }
}
