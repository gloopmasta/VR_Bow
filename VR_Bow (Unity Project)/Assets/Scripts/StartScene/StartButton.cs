using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StartButton : MonoBehaviour
{
    [SerializeField] private LevelEventsSO levelEvents;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrow")) //if arrow shoots
        {
           levelEvents.RaiseLevelOneStart();
        }

    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 300, 40), "start game"))
        {
            levelEvents.RaiseLevelOneStart();
        }
    }
}
