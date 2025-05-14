using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    [Header("Timing Settings")]
    [SerializeField] private float warningTime = 0.5f;
    [SerializeField] private float gapTime = 0.1f;
    [SerializeField] private float activeTime = 2f;

    [Header("Laser Settings")]
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private Color warningColor = Color.white;
    [SerializeField] private Color activeColor = Color.red;

    public Transform headToTurn;
    public Action onLaserComplete;

    private LineRenderer lineRenderer;
    private Vector3 startPoint;
    private Vector3 endPoint;
    public Vector3 fireDirection;

    private Material emissiveMat;
    private const float glowIntensity = 10f;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        emissiveMat = new Material(Shader.Find("Unlit/Color"));
        emissiveMat.SetColor("_Color", warningColor * 0f);
        lineRenderer.material = emissiveMat;

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        warningColor.a = 1f;
        activeColor.a = 1f;

        fireDirection = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            0f,
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized;

        startPoint = transform.position;
        endPoint = startPoint + fireDirection * maxDistance;

        if (headToTurn != null)
        {
            Quaternion lookRotation = Quaternion.LookRotation(fireDirection, Vector3.up);
            headToTurn.rotation = lookRotation;
        }

        FireLaser().Forget();
    }

    private async UniTaskVoid FireLaser()
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);

        // Fade-in
        float t = 0f;
        while (t < warningTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / warningTime);

            Color fadedColor = Color.Lerp(Color.black, warningColor, alpha);
            emissiveMat.SetColor("_Color", fadedColor * glowIntensity);

            lineRenderer.startWidth = 0.1f + alpha * 0.1f;
            lineRenderer.endWidth = 0.1f + alpha * 0.1f;

            await UniTask.Yield();
        }

        // Gap
        lineRenderer.enabled = false;
        await UniTask.Delay(TimeSpan.FromSeconds(gapTime));

        // Active phase
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
        emissiveMat.SetColor("_Color", activeColor * glowIntensity);

        RaycastHit[] hits = Physics.RaycastAll(startPoint, fireDirection, maxDistance);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<IDamageable>()?.TakeDamage(1);
            }
        }

        await UniTask.Delay(TimeSpan.FromSeconds(activeTime));

        // Fade-out phase
        float fadeTime = warningTime;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / fadeTime));

            Color fadedColor = Color.Lerp(Color.black, activeColor, alpha);
            emissiveMat.SetColor("_Color", fadedColor * glowIntensity);

            lineRenderer.startWidth = 0.2f * alpha;
            lineRenderer.endWidth = 0.2f * alpha;

            await UniTask.Yield();
        }

        lineRenderer.enabled = false;
        onLaserComplete?.Invoke();
        await UniTask.Yield(); // Just to be extra sure fade is done
        Destroy(gameObject);

    }
}
