using UnityEngine;

public class RaceFinishTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Adjust if needed
        {
            Debug.Log("Player reached finish Line");
            RaceTrack raceTrack = GetComponentInParent<RaceTrack>();
            if (raceTrack != null)
            {
                raceTrack.OnPlayerReachedFinish();
            }
        }
    }
}
