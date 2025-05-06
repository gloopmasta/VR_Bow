using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    public float warningTime = 0.5f;
    public float gapTime = 0.1f;
    public float activeTime = 2f;
    public float damageRadius = 0.5f;
    public float maxDistance = 20f;

    public Color warningColor = Color.white;
    public Color activeColor = Color.red * 2f;

    private LineRenderer lr;
    private Material laserMat;
    private Vector3 start;
    private Vector3 end;
    private Vector3 fireDirection;

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        laserMat = lr.material;

        // Kies random horizontale richting
        fireDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        start = transform.position;
        end = start + fireDirection * maxDistance;

        StartCoroutine(FireLaser());
    }

    private IEnumerator FireLaser()
    {
        // 1. Witte waarschuwingslijn, uitschuivend
        lr.enabled = true;
        laserMat.SetColor("_EmissionColor", warningColor);
        lr.SetPosition(0, start);
        lr.SetPosition(1, start);

        float t = 0f;
        while (t < warningTime)
        {
            t += Time.deltaTime;
            Vector3 lerped = Vector3.Lerp(start, end, t / warningTime);
            lr.SetPosition(1, lerped);
            yield return null;
        }

        // 2. Pauze (laser verdwijnt)
        lr.enabled = false;
        yield return new WaitForSeconds(gapTime);

        // 3. Rode laser actief
        lr.enabled = true;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        laserMat.SetColor("_EmissionColor", activeColor);

        // 4. Damage toepassen
        RaycastHit[] hits = Physics.SphereCastAll(start, damageRadius, fireDirection, maxDistance);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<IDamageable>()?.TakeDamage(1);
            }
        }

        yield return new WaitForSeconds(activeTime);
        Destroy(gameObject);
    }
}
