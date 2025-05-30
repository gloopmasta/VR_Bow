using System.Collections;
using UnityEngine;

public class BowEmissionController : MonoBehaviour
{
    [Header("Renderer & Material")]
    [SerializeField] private Renderer bowRenderer;

    [Header("Colors")]
    [SerializeField] private Color idleColor = Color.black;
    [SerializeField] private Color chargingColor = Color.blue;
    [SerializeField] private Color shootColor = Color.white;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private float lerpSpeed = 5f;

    private Material bowMaterial;
    private Color targetColor;
    private Coroutine flashRoutine;

    private void OnEnable()
    {
        bowMaterial = bowRenderer.material;
        bowMaterial.EnableKeyword("_EMISSION");

        BowVisualEvents.OnChargeLevelChanged += HandleChargeLevelChanged;
        BowVisualEvents.OnArrowReleased += HandleArrowReleased;
        BowVisualEvents.OnBowIdle += HandleIdle;
    }

    private void OnDisable()
    {
        BowVisualEvents.OnChargeLevelChanged -= HandleChargeLevelChanged;
        BowVisualEvents.OnArrowReleased -= HandleArrowReleased;
        BowVisualEvents.OnBowIdle -= HandleIdle;
    }

    private void Update()
    {
        Color currentColor = bowMaterial.GetColor("_EmissionColor");
        Color newColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * lerpSpeed);
        bowMaterial.SetColor("_EmissionColor", newColor);
    }

    private void HandleChargeLevelChanged(float value)
    {
        targetColor = Color.Lerp(shootColor, chargingColor, value);
    }

    private void HandleArrowReleased()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashColor());
    }

    private void HandleIdle()
    {
        targetColor = idleColor;
    }

    private IEnumerator FlashColor()
    {
        bowMaterial.SetColor("_EmissionColor", shootColor);
        yield return new WaitForSeconds(flashDuration);
        targetColor = idleColor;
    }
}
