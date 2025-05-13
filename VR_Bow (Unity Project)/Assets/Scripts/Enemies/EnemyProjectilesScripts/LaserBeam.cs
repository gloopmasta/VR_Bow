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

    private LineRenderer lineRenderer;
    private Vector3 startPoint;
    private Vector3 endPoint;
    public Vector3 fireDirection;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Set up the line renderer with default appearance
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Ensures transparency
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // Set initial transparency
        warningColor.a = 0f;
        activeColor.a = 1f;
         
        // Generate random direction in XZ plane
        fireDirection = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            0f,
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized;

        startPoint = transform.position;
        endPoint = startPoint + fireDirection * maxDistance;

        FireLaser().Forget();
    }

    private async UniTaskVoid FireLaser()
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);

        // Warning phase: fade in white laser
        float t = 0f;
        while (t < warningTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / warningTime);

            Color fadedColor = warningColor;
            fadedColor.a = alpha;

            lineRenderer.startColor = fadedColor;
            lineRenderer.endColor = fadedColor;

            lineRenderer.startWidth = 0.1f + alpha * 0.1f;
            lineRenderer.endWidth = 0.1f + alpha * 0.1f;

            await UniTask.Yield();
        }

        // Gap phase: laser temporarily invisible
        lineRenderer.enabled = false;
        await UniTask.Delay(TimeSpan.FromSeconds(gapTime));

        // Active phase: show red laser and apply damage
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);

        lineRenderer.startColor = activeColor;
        lineRenderer.endColor = activeColor;

        RaycastHit[] hits = Physics.RaycastAll(startPoint, fireDirection, maxDistance);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<IDamageable>()?.TakeDamage(1);
            }
        }

        // Wait for the active duration, then destroy this object
        await UniTask.Delay(TimeSpan.FromSeconds(activeTime));
        Destroy(gameObject);
    }
}
