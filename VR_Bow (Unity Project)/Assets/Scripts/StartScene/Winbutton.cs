using UnityEngine;
using UnityEngine.SceneManagement;

public class Winbutton : MonoBehaviour
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
