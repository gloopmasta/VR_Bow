using UnityEngine;
using UnityEngine.SceneManagement;

public class Winbutton : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrow")) // als de arrow dit object raakt
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // herlaad het huidige level
        }
    }
}
