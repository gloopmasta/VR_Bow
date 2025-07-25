using UnityEngine;

public class BowModelHandler : MonoBehaviour
{
    [SerializeField] private Transform leftController;
    [SerializeField] private Transform lockPoint;
    [SerializeField] private Vector3 bowOffsetEuler; // If needed
    [SerializeField] private float maxVisibleRotation = 50f; // If needed
    public bool isLocked;

    [SerializeField] LineRenderer trajectoryLineRenderer;
    

    void LateUpdate()
    {
        if (isLocked)
        {
            // Position is locked
            transform.position = lockPoint.position;

            // Get controller rotation
            Vector3 controllerEuler = leftController.rotation.eulerAngles;

            // Use only Y and Z rotation
            float clampedZ = Mathf.Clamp(-controllerEuler.z, -maxVisibleRotation, maxVisibleRotation);

            Quaternion limitedRotation = Quaternion.Euler(-controllerEuler.z, 90f, 0f);
            // LEFT HANDED -> Quaternion limitedRotation = Quaternion.Euler(controllerEuler.z, 90f, 0f);
            transform.localRotation = limitedRotation;
        }
        else
        {
            // Follow full position and rotation of controller
            transform.position = leftController.position;
            transform.rotation = leftController.rotation * Quaternion.Euler(bowOffsetEuler);
        }
    }
}
