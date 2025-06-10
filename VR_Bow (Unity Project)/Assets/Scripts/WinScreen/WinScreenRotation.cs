using UnityEngine;

public class WinScreenRotation : MonoBehaviour
{
    private void OnEnable()
    {
        // Make the screen face the player (but stay upright)
        Vector3 lookDirection = Camera.main.transform.position - transform.position;
        lookDirection.y = 0; // Keep the screen upright (don't tilt up/down)

        transform.rotation = Quaternion.LookRotation(lookDirection);
    }
}
