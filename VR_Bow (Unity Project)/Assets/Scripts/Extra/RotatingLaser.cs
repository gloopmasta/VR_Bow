using UnityEngine;
using System.Collections.Generic;

public class RotatingLaser : MonoBehaviour, ITimeScalable
{
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float laserLength = 5f;
    [SerializeField] private int numberOfLasers = 1;
    [SerializeField] private string spawnedByTag;

    [SerializeField] private bool spinVertically = false;
    [SerializeField] private Color laserColor = Color.red;
    [SerializeField] private float glowIntensity = 10f;

    private GameObject laserPivot;
    private List<Transform> laserTransforms = new List<Transform>();
    private float timeScale = 1f;

    private void Start()
    {
        if (string.IsNullOrEmpty(spawnedByTag))
        {
            spawnedByTag = transform.root.tag;
        }

        laserPivot = new GameObject("LaserPivot");
        laserPivot.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
        if (spinVertically)
            laserPivot.transform.SetParent(transform);

        float angleStep = 360f / numberOfLasers;

        for (int i = 0; i < numberOfLasers; i++)
        {
            GameObject laserObj = new GameObject($"Laser_{i}");
            laserObj.transform.SetParent(laserPivot.transform);
            laserObj.transform.localPosition = Vector3.zero;
            laserObj.transform.localRotation = Quaternion.identity;

            if (spinVertically)
                laserObj.transform.Rotate(Vector3.right, angleStep * i);
            else
                laserObj.transform.Rotate(Vector3.up, angleStep * i);

            LineRenderer lr = laserObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;

            Material emissiveMat = new Material(Shader.Find("Unlit/Color"));
            emissiveMat.SetColor("_Color", laserColor * glowIntensity);
            lr.material = emissiveMat;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;

            BoxCollider box = laserObj.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0, 0, laserLength / 2f);
            box.size = new Vector3(0.1f, 0.1f, laserLength);

            laserObj.AddComponent<LaserTrigger>().Initialize(this, i);

            laserTransforms.Add(laserObj.transform);
        }

        GameManager.Instance?.Register(this);
    }

    private void Update()
    {
        if (!spinVertically)
        {
            laserPivot.transform.position = transform.position;
        }

        Vector3 axis = spinVertically ? Vector3.right : Vector3.up;
        Space space = spinVertically ? Space.Self : Space.World;
        laserPivot.transform.Rotate(axis, rotationSpeed * timeScale * Time.deltaTime, space);

        for (int i = 0; i < numberOfLasers; i++)
        {
            Transform t = laserTransforms[i];
            Vector3 start = t.position;
            Vector3 end = start + t.forward * laserLength;

            LineRenderer lr = t.GetComponent<LineRenderer>();
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
    }

    public void OnTimeScaleChanged(float newScale)
    {
        timeScale = newScale;
    }

    private void OnDestroy()
    {
        if (laserPivot != null) Destroy(laserPivot);
        GameManager.Instance?.Unregister(this);
    }

    public void HandleTrigger(Collider other, int laserIndex)
    {
        if (other.transform.IsChildOf(laserTransforms[laserIndex]))
            return;

        if (spawnedByTag == "Player" && other.CompareTag("Enemy"))
        {
            other.GetComponent<IDamageable>()?.TakeDamage(1);
        }
        else if (spawnedByTag == "Enemy" && other.CompareTag("Player"))
        {
            other.GetComponent<IDamageable>()?.TakeDamage(1);
            Debug.Log($"Hit Player with laser {laserIndex}");
        }
    }

    private class LaserTrigger : MonoBehaviour
    {
        private RotatingLaser laserParent;
        private int index;

        public void Initialize(RotatingLaser parent, int idx)
        {
            laserParent = parent;
            index = idx;
        }

        private void OnTriggerEnter(Collider other)
        {
            laserParent?.HandleTrigger(other, index);
        }
    }
}
