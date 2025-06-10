using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartButton : MonoBehaviour
{
    [SerializeField] LevelEventsSO levelEvents;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("something entered the restart button: " + other.name);
        if (other.CompareTag("Arrow")) // als de arrow dit object raakt
        {
            Debug.Log("it was arrow -> raiseLevelOneRestart");
            levelEvents.RaiseLevelOneRestart();
        }
    }
}
