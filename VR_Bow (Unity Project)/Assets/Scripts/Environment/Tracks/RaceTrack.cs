using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceTrack : MonoBehaviour
{
    [SerializeField] Material trackMaterial;

    private void OnEnable()
    {
        DeactivateAllPowerups();
    }

    public void ActivateAllPowerups()
    {
        Powerup[] powerups = GetComponentsInChildren<Powerup>(includeInactive: true);

        foreach (var powerup in powerups)
        {
            powerup.gameObject.SetActive(true);
            // Optionally call a method if you want a specific "Activate" behavior
            // powerup.Activate(); // Uncomment if Powerup defines an Activate() method
        }
    }

    public void DeactivateAllPowerups()
    {
        Powerup[] powerups = GetComponentsInChildren<Powerup>(includeInactive: true);

        foreach (var powerup in powerups)
        {
            powerup.gameObject.SetActive(false);
            // Optionally call a method if you want a specific "Activate" behavior
            // powerup.Activate(); // Uncomment if Powerup defines an Activate() method
        }
    }

    private void ChangeTrackMaterial()
    {
        Material newTrackmaterial = trackMaterial;

        
    }

    public void Activate()
    {
        ActivateAllPowerups();

    }
}
