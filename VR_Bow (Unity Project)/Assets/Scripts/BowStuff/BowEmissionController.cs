using System.Collections;
using UnityEngine;

public class BowEmissionController : MonoBehaviour
{
    [SerializeField] BowEventsSO bowEvents;

    [Header("Renderer & Material")]
    [SerializeField] private Renderer bowRenderer;

    [Header("Colors")]
    [SerializeField] private Color idleColor = Color.black;
    [SerializeField] private Color chargingColor = Color.blue;
    [SerializeField] private Color shootColor = Color.white;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private float emissionIntensity = 5f; // <-- New: Intensity multiplier

    private bool isIdlePulsing = false;
    [SerializeField] private Color idleColorA = Color.green;
    [SerializeField] private Color idleColorB = Color.white;
    [SerializeField] private float idlePulseSpeed = 2f;

    [SerializeField] private Material bowMaterial;
    private Color targetColor;
    private Coroutine flashRoutine;

    private void OnEnable()
    {
        bowMaterial = bowRenderer.material;
        bowMaterial.EnableKeyword("_EMISSION");

        // Set initial emission so it's visible
        targetColor = idleColor;
        bowMaterial.SetColor("_EmissionColor", idleColor * emissionIntensity);

        bowEvents.OnChargeLevelChanged += HandleChargeLevelChanged;
        bowEvents.OnArrowReleased += HandleArrowReleased;
        bowEvents.OnBowIdle += HandleIdle;
    }

    private void OnDisable()
    {
        bowEvents.OnChargeLevelChanged -= HandleChargeLevelChanged;
        bowEvents.OnArrowReleased -= HandleArrowReleased;
        bowEvents.OnBowIdle -= HandleIdle;
    }

    private void Update()
    {
        if (bowMaterial == null) return;

        if (isIdlePulsing)
        {
            float t = (Mathf.Sin(Time.time * idlePulseSpeed) + 1f) / 2f;
            Color pulseColor = Color.Lerp(idleColorA, idleColorB, t);
            bowMaterial.SetColor("_EmissionColor", pulseColor * emissionIntensity);
        }
        else
        {
            Color currentColor = bowMaterial.GetColor("_EmissionColor");
            Color targetEmission = targetColor * emissionIntensity;
            Color newColor = Color.Lerp(currentColor, targetEmission, Time.deltaTime * lerpSpeed);
            bowMaterial.SetColor("_EmissionColor", newColor);
        }
    }

    private void HandleChargeLevelChanged(float value)
    {
        isIdlePulsing = false;
        targetColor = Color.Lerp(shootColor, chargingColor, value);
    }

    private void HandleArrowReleased()
    {
        isIdlePulsing = false;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashColor());
    }

    private void HandleIdle()
    {
        isIdlePulsing = true;
    }

    private IEnumerator FlashColor()
    {
        bowMaterial.SetColor("_EmissionColor", shootColor * emissionIntensity);
        yield return new WaitForSeconds(flashDuration);
        targetColor = idleColor;
    }
}
