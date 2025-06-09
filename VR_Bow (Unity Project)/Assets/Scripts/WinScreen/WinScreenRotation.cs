using UnityEngine;

public class WinScreenRotation : MonoBehaviour
{
    private void OnEnable()
    {
        Vector3 targetRotation = new Vector3(0f, Camera.main.transform.rotation.y, 0f); // get camera y rotation

        transform.rotation = Quaternion.Euler(targetRotation);
    }
}
