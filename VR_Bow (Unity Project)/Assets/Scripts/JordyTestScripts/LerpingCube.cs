using UnityEngine;
using UnityEngine.InputSystem;

public class NewBehaviourScript : MonoBehaviour
{
    [Tooltip("Time scale to use during slow motion (e.g. 0.2 for 20% speed)")]
    [Range(0f, 1f)]
    public float slowTimeScale = 0.2f;

    private bool isSlowed = false;

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ToggleSlowMotion();
        }
    }

    void ToggleSlowMotion()
    {
        isSlowed = !isSlowed;

        if (isSlowed)
        {
            Time.timeScale = slowTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        else
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }
}
