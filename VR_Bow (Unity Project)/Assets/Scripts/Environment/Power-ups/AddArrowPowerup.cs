using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddArrowPowerup : Powerup
{
    [SerializeField] int arrowsToAdd = 1;
    protected override void ApplyEffect(GameObject player)
    {
        var playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.ArrowCount += arrowsToAdd;
            Debug.Log("Added Arrows");
        }
    }
}
