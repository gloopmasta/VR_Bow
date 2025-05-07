using UnityEngine;

public class TrailSpawner : MonoBehaviour
{
    public GameObject trailSegmentPrefab; // optioneel invulbaar in inspector

    private float segmentSpacing = 0.01f;
    private Vector3 lastPosition;
    private bool isSpawning = false;

    private void OnEnable()
    {
        // Probeer prefab te laden uit Resources als deze niet is ingevuld
        if (trailSegmentPrefab == null)
        {
            trailSegmentPrefab = Resources.Load<GameObject>("TrailSegment");

            if (trailSegmentPrefab == null)
            {
                Debug.LogError("TronTrailSpawner: Geen trailSegmentPrefab ingevuld of gevonden in Resources folder!");
                enabled = false;
                return;
            }
        }

        isSpawning = true;
        lastPosition = transform.position;
    }

    void Update()
    {
        if (!isSpawning || trailSegmentPrefab == null) return;

        float distance = Vector3.Distance(transform.position, lastPosition);
        if (distance >= segmentSpacing)
        {
            SpawnSegment(transform.position);
            lastPosition = transform.position;
        }
    }

    void SpawnSegment(Vector3 position)
    {
        Vector3 direction = position - lastPosition;
        float distance = direction.magnitude;
        if (distance == 0) return;

        Vector3 midPoint = lastPosition + (direction / 2);

        GameObject segment = Instantiate(trailSegmentPrefab, midPoint, Quaternion.LookRotation(direction));

        segment.transform.localScale = new Vector3(segment.transform.localScale.x, segment.transform.localScale.y, distance);


        TrailSegment trail = segment.GetComponent<TrailSegment>();
        if (trail != null)
        {
            trail.spawnedByTag = gameObject.tag;
        }
    }


}
