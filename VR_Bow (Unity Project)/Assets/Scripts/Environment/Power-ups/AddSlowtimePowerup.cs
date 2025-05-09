using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddSlowtimePowerup : Powerup
{
    [Header("Powerup-Specific Settings")]
    [SerializeField] float slowtimeToAdd = 2f;

    protected override void ApplyEffect(GameObject player)
    {
        var playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.SlowTime += slowtimeToAdd;
            Debug.Log($"Added {slowtimeToAdd}s Slowtime to {player.gameObject.name}");
        }
    }
}
