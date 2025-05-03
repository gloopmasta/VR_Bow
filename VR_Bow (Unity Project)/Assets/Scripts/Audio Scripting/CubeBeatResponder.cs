using UnityEngine;

public class CubeBeatResponder : MonoBehaviour
{
    private Renderer cubeRenderer;
    private Vector3 originalScale;
    private Color originalColor;

    [SerializeField] private float scaleMultiplier = 1.5f;
    [SerializeField] private float effectDuration = 0.2f;
    private float timer;

    void Start()
    {
        cubeRenderer = GetComponent<Renderer>();
        originalScale = transform.localScale;
        originalColor = cubeRenderer.material.color;

        BeatManager.OnBeatChange += RespondToBeat;
    }

    void OnDestroy()
    {
        BeatManager.OnBeatChange -= RespondToBeat;
    }

    void RespondToBeat()
    {
        transform.localScale = originalScale * scaleMultiplier;
        cubeRenderer.material.color = Random.ColorHSV(); // change to random color
        timer = effectDuration;
    }

    void Update()
    {
        // Gradually return to normal scale
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                transform.localScale = originalScale;
                cubeRenderer.material.color = originalColor;
            }
        }
    }
}
