using UnityEngine;

public class StartLineTrigger : MonoBehaviour
{
    [SerializeField] private RaceTrack track; // Reference to the main track script
    [SerializeField] private float activationAngle = 30f;

    private bool activated = false;

    private void OnEnable()
    {
        track = GetComponentInParent<RaceTrack>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            Transform playerTransform = other.transform;

            // Check angle between player forward and start line forward
            float angle = Vector3.Angle(transform.forward, playerTransform.forward);

            if (angle <= activationAngle)
            {
                ActivateTrack(other.gameObject);
            }
        }
    }

    private void ActivateTrack(GameObject player)
    {
        activated = true;

        //assign playerUimanager in TRack script
        if (player.GetComponent<PlayerUIManager>() != null)
            track.playerUI = player.GetComponent<PlayerUIManager>();
        else
            Debug.LogWarning("No PlayerUIManager found in collision");

        // Activate track
        track.Activate();
        
    }
}
