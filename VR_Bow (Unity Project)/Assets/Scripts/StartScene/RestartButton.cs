using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartButton : MonoBehaviour
{
    [SerializeField] LevelEventsSO levelEvents;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrow")) // als de arrow dit object raakt
        {
            levelEvents.RaiseLevelOneRestart();
        }
    }
}
