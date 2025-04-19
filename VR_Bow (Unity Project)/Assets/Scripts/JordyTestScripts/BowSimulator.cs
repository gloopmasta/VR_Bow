using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class BowSimulator : MonoBehaviour
{
    public GameObject projectilePrefab;        // Prefab of the arrow or object to shoot
    public Transform shootPoint;               // Position and direction to shoot from
    public float maxShootForce = 20f;          // Maximum force applied when fully stretched

    public InputActionAsset inputActions;      // Reference to the Input Actions asset in the Inspector

    private InputAction flexAction;            // Controls the stretch input
    private InputAction setMinAction;          // Button to set the relaxed position (min)
    private InputAction setMaxAction;          // Button to set the full draw position (max)
    private InputAction shootAction;           // Button to shoot

    private float flexValue = 0f;              // Current simulated flex amount
    private float minValue = 0f;               // Saved minimum flex position
    private float maxValue = 1f;               // Saved maximum flex position

    void OnEnable()
    {
        // Find the action map by name — must match the one in the Input Actions asset
        var gameplayMap = inputActions.FindActionMap("VrPlayerController");

        // Find individual actions by name — make sure they exist in the action map
        flexAction = gameplayMap.FindAction("Flex");
        setMinAction = gameplayMap.FindAction("SetMin");
        setMaxAction = gameplayMap.FindAction("SetMax");
        shootAction = gameplayMap.FindAction("Shoot");

        // Enable the whole map to start listening for input
        gameplayMap.Enable();

        // Register callbacks for the button actions
        setMinAction.performed += ctx => SetMin();
        setMaxAction.performed += ctx => SetMax();
        shootAction.performed += ctx => Shoot();
    }

    void OnDisable()
    {
        // Disable all actions when this component is disabled to avoid memory leaks
        flexAction.Disable();
        setMinAction.Disable();
        setMaxAction.Disable();
        shootAction.Disable();
    }

    void Update()
    {
        // Read the 1D axis input from the flex action (e.g. arrow keys or analog)
        float axisInput = flexAction.ReadValue<float>();

        // Adjust the flex value based on input, clamped between 0 and 1
        flexValue = Mathf.Clamp01(flexValue + axisInput * Time.deltaTime);

        Debug.Log($"Flex Value: {flexValue:F2}");
    }

    void SetMin()
    {
        // Save the current flex value as the minimum stretch position
        minValue = flexValue;
        Debug.Log($"[INPUT] Min set to: {minValue:F2}");
    }

    void SetMax()
    {
        // Save the current flex value as the maximum stretch position
        maxValue = flexValue;
        Debug.Log($"[INPUT] Max set to: {maxValue:F2}");
    }

    void Shoot()
    {
        // Calculate stretch ratio between min and max flex positions
        float flexRatio = Mathf.InverseLerp(minValue, maxValue, flexValue);

        // Convert that ratio into a shoot force
        float shootForce = flexRatio * maxShootForce;

        Debug.Log($"[INPUT] SHOOT! Ratio: {flexRatio:F2}, Force: {shootForce:F2}");

        // Instantiate and launch the projectile
        GameObject arrow = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        rb.velocity = shootPoint.forward * shootForce;
    }
}
