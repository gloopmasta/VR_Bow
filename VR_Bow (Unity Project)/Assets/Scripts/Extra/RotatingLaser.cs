using UnityEngine;
using System.Collections.Generic;

public class RotatingLaser : MonoBehaviour, ITimeScalable
{
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float laserLength = 5f;
    [SerializeField] private int numberOfLasers = 1;
    [SerializeField] private string spawnedByTag;

    [SerializeField] private bool spinVertically = false; // toggle vertical/horizontal

    private GameObject laserPivot;
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private float nextDamageTime;

    [SerializeField] private Color laserColor = Color.red;
    [SerializeField] private float glowIntensity = 10f;

    private float timeScale = 1f;

    private void Start()
    {
        if (string.IsNullOrEmpty(spawnedByTag))
        {
            spawnedByTag = transform.root.tag;
        }

        // Create pivot object for rotation
        laserPivot = new GameObject("LaserPivot");

        if (spinVertically)
        {
            // Attach to this object so you can rotate the prefab in editor
            laserPivot.transform.SetParent(transform);
            laserPivot.transform.localPosition = Vector3.zero;
            laserPivot.transform.localRotation = Quaternion.identity;
        }
        else
        {
            // Spawn separately
            laserPivot.transform.position = transform.position;
            laserPivot.transform.rotation = Quaternion.identity;
            laserPivot.transform.SetParent(null);
        }

        float angleStep = 360f / numberOfLasers;

        for (int i = 0; i < numberOfLasers; i++)
        {
            GameObject laserObj = new GameObject($"Laser_{i}");
            laserObj.transform.SetParent(laserPivot.transform);
            laserObj.transform.localPosition = Vector3.zero;
            laserObj.transform.localRotation = Quaternion.identity;

            if (spinVertically)
                laserObj.transform.Rotate(Vector3.right, angleStep * i);  // Spread vertically
            else
                laserObj.transform.Rotate(Vector3.up, angleStep * i);     // Spread horizontally

            LineRenderer lr = laserObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;

            Material emissiveMat = new Material(Shader.Find("Unlit/Color"));
            emissiveMat.SetColor("_Color", laserColor * glowIntensity);
            lr.material = emissiveMat;

            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;

            lineRenderers.Add(lr);
        }

        GameManager.Instance.Register(this);
    }

    private void Update()
    {
        // Update pivot position if it's world-based
        if (!spinVertically)
        {
            laserPivot.transform.position = transform.position;
        }

        // Choose axis and space
        Vector3 axis = spinVertically ? Vector3.right : Vector3.up;
        Space space = spinVertically ? Space.Self : Space.World;

        laserPivot.transform.Rotate(axis, rotationSpeed * timeScale * Time.deltaTime, space);

        for (int i = 0; i < numberOfLasers; i++)
        {
            Transform laserTransform = laserPivot.transform.GetChild(i);
            Vector3 direction = laserTransform.forward;
            Vector3 start = laserTransform.position;
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
                        nextDamageTime = Time.time + 1f / timeScale;
                    }
                    else if (spawnedByTag == "Enemy" && hit.collider.CompareTag("Player"))
                    {
                        hit.collider.GetComponent<IDamageable>()?.TakeDamage(1);
                        nextDamageTime = Time.time + 1f / timeScale;
                    }
                }
            }
        }
    }

    public void OnTimeScaleChanged(float newScale)
    {
        timeScale = newScale;
    }

    private void OnDestroy()
    {
        if (laserPivot != null)
        {
            Destroy(laserPivot);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Unregister(this);
        }
    }
}
