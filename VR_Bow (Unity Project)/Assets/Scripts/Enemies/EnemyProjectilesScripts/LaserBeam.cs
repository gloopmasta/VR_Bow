using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    public float warningTime = 0.5f;
    public float gapTime = 0.1f;
    public float activeTime = 2f;
    public float maxDistance = 20f;

    public Color warningColor = Color.white;
    public Color activeColor = Color.red;

    private LineRenderer lr;
    private Vector3 start;
    private Vector3 end;
    private Vector3 fireDirection;

    private void Start()
    {
        lr = GetComponent<LineRenderer>();

        // Zorg ervoor dat het materiaal de juiste transparantie ondersteunt
        lr.material = new Material(Shader.Find("Sprites/Default")); // Zorgt ervoor dat transparantie mogelijk is
        lr.startWidth = 0.1f; // Pas dit aan voor de gewenste lijnbreedte
        lr.endWidth = 0.1f;

        // Stel de begin- en eindkleur in met transparantie
        warningColor.a = 0f; // Start invisible
        activeColor.a = 1f;

        fireDirection = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            0f,
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized;

        start = transform.position;
        end = start + fireDirection * maxDistance;

        FireLaser().Forget();
    }

    private async UniTaskVoid FireLaser()
    {
        lr.enabled = true;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // Waarschuwingsfase (fade-in naar wit)
        float t = 0f;
        while (t < warningTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / warningTime);

            Color faded = warningColor;
            faded.a = alpha;

            lr.startColor = faded;
            lr.endColor = faded;

            // Zorg ervoor dat de lijnbrede correct wordt ingesteld, mogelijk opnieuw instellen
            lr.startWidth = 0.1f + alpha * 0.1f;  // Verhoog de breedte beetje per beetje
            lr.endWidth = 0.1f + alpha * 0.1f;

            await UniTask.Yield();
        }

        // Pauze (lijn wordt tijdelijk uitgeschakeld)
        lr.enabled = false;
        await UniTask.Delay(TimeSpan.FromSeconds(gapTime));

        // Actieve rode laser (met schade)
        lr.enabled = true;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        lr.startColor = activeColor;
        lr.endColor = activeColor;

        // Gebruik een Raycast om de schade toe te passen
        RaycastHit[] hits = Physics.RaycastAll(start, fireDirection, maxDistance);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<IDamageable>()?.TakeDamage(1);
            }
        }

        // Wacht de actieve tijd af voordat de laser wordt vernietigd
        await UniTask.Delay(TimeSpan.FromSeconds(activeTime));
        Destroy(gameObject);
    }
}
