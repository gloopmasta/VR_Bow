using UnityEngine;
using System.Collections.Generic;

public class RotatingLaser : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float laserLength = 5f;
    [SerializeField] private int numberOfLasers = 1;
    [SerializeField] private string spawnedByTag;

    private GameObject laserPivot;
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private float nextDamageTime;

    // Emissive kleur en intensiteit
    [SerializeField] private Color laserColor = Color.red;
    [SerializeField] private float glowIntensity = 10f; // Zorg dat Bloom deze oppakt

    private void Start()
    {
        // Create pivot
        laserPivot = new GameObject("LaserPivot");
        laserPivot.transform.position = transform.position;
        laserPivot.transform.rotation = Quaternion.identity;
        laserPivot.transform.SetParent(null);

        for (int i = 0; i < numberOfLasers; i++)
        {
            GameObject laserObj = new GameObject($"Laser_{i}");
            laserObj.transform.SetParent(laserPivot.transform);
            laserObj.transform.localPosition = Vector3.zero;
            laserObj.transform.localRotation = Quaternion.identity;

            LineRenderer lr = laserObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;

            // Maak emissive materiaal
            Material emissiveMat = new Material(Shader.Find("Unlit/Color"));
            emissiveMat.SetColor("_Color", laserColor * glowIntensity);
            lr.material = emissiveMat;

            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;

            lineRenderers.Add(lr);
        }
    }

    private void Update()
    {
        // Pivot draaien
        laserPivot.transform.position = transform.position;
        laserPivot.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        float angleStep = 360f / numberOfLasers;

        for (int i = 0; i < numberOfLasers; i++)
        {
            Quaternion rotationOffset = Quaternion.Euler(0f, angleStep * i, 0f);
            Vector3 direction = rotationOffset * laserPivot.transform.forward;

            Vector3 start = laserPivot.transform.position;
            Vector3 end = start + direction * laserLength;

            lineRenderers[i].SetPosition(0, start);
            lineRenderers[i].SetPosition(1, end);

            if (Physics.Raycast(start, direction, out RaycastHit hit, laserLength))
            {
                if (Time.time >= nextDamageTime)
                {
                    if (spawnedByTag == "Player" && hit.collider.CompareTag("Enemy"))
                    {
                        hit.collider.GetComponent<IDamageable>()?.TakeDamage(1);
                        nextDamageTime = Time.time + 1f;
                    }
                    else if (spawnedByTag == "Enemy" && hit.collider.CompareTag("Player"))
                    {
                        hit.collider.GetComponent<IDamageable>()?.TakeDamage(1);
                        nextDamageTime = Time.time + 1f;
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (laserPivot != null)
        {
            Destroy(laserPivot);
        }
    }
}
