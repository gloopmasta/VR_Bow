using UnityEngine;
using System.Collections.Generic;

public class RotatingLaser : MonoBehaviour, ITimeScalable
{
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float laserLength = 5f;
    [SerializeField] private int numberOfLasers = 1;
    [SerializeField] private string spawnedByTag;
    [SerializeField] private bool spinAnimation = false;
    [SerializeField] private GameObject partToSpin;

    [SerializeField] private bool spinVertically = false;
    [SerializeField] private Color laserColor = Color.red;
    [SerializeField] private float glowIntensity = 10f;
    [SerializeField] private Material laserMat;

    [SerializeField] private AudioCue humSoundCue;  // <- Dit is nu je AudioCue asset

    private GameObject laserPivot;
    private List<Transform> laserTransforms = new List<Transform>();
    private List<GameObject> soundObjects = new List<GameObject>();

    private float timeScale = 1f;

    private void Awake()
    {
        laserPivot = new GameObject("LaserPivot");
        laserPivot.transform.SetPositionAndRotation(transform.position, Quaternion.identity);

        if (spinVertically)
            laserPivot.transform.SetParent(transform);
    }


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

            
            lr.material = laserMat;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;

            BoxCollider box = laserObj.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0, 0, laserLength / 2f);
            box.size = new Vector3(0.1f, 0.1f, laserLength);

            laserObj.AddComponent<LaserTrigger>().Initialize(this, i);

            laserTransforms.Add(laserObj.transform);

            // Gebruik AudioCue om geluid te spelen op het midden van elke laser
            if (SoundEffectManager.Instance != null && humSoundCue != null)
            {
                Vector3 soundPos = laserObj.transform.position + laserObj.transform.forward * (laserLength / 2f);
                GameObject sfx = SoundEffectManager.Instance.Play3D(humSoundCue, soundPos, laserObj.transform, loop: true);
                if (sfx != null)
                    soundObjects.Add(sfx);
            }
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
        partToSpin.transform.Rotate(axis, rotationSpeed * timeScale * Time.deltaTime, space);

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

        // Vernietig alle sound objects
        foreach (var sfx in soundObjects)
        {
            if (sfx != null)
                Destroy(sfx);
        }
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
