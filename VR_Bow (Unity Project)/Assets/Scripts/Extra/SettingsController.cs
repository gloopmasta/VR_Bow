using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    public VisualTreeAsset uiAsset; 
    private UIDocument uiDocument;
    private InputAction toggleAction;

    private int comValue = 0;
    private bool bowMode = false;

    private TextField comField;
    private Toggle bowToggle;
    private Button restartLevelButton;
    private Button restartIntroButton;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("No UIDocument found on GameObject.");
            return;
        }

        uiDocument.enabled = true;

        var root = uiDocument.rootVisualElement;

        // Ensure the root starts hidden via style, not GameObject state
        root.style.display = DisplayStyle.None;

        // Query UI elements by their names
        comField = root.Q<TextField>("ComField");
        bowToggle = root.Q<Toggle>("BowMode");
        restartLevelButton = root.Q<Button>("RestartLevel1");
        restartIntroButton = root.Q<Button>("RestartIntro");

        if (comField != null)
            comField.RegisterValueChangedCallback(OnComFieldChanged);

        if (bowToggle != null)
            bowToggle.RegisterValueChangedCallback(evt => gameSettings.useBowController = evt.newValue);

        if (restartLevelButton != null)
            restartLevelButton.clicked += RestartLevelOne;

        if (restartIntroButton != null)
            restartLevelButton.clicked += RestartIntro;

        toggleAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/escape");
        toggleAction.performed += ctx => ToggleUI();
        toggleAction.Enable();
    }


    private void ToggleUI()
    {
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            root.style.display = root.style.display == DisplayStyle.None
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }
    }

    private void OnComFieldChanged(ChangeEvent<string> evt)
    {
        if (int.TryParse(evt.newValue, out int parsed))
        {
            comValue = parsed;
            Debug.Log("Updated comValue to: COM" + comValue);

            gameSettings.comPort = "COM" + comValue;
        }
        else
        {
            Debug.LogWarning("Invalid integer input: " + evt.newValue);
            // Optionally revert to previous value or clear
        }
    }

    private void RestartLevelOne()
    {
        SceneManager.LoadScene(1);
    }

    private void RestartIntro()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        //SceneManager.LoadScene("IntroScene");
    }

    // Optional: expose comValue and bowMode
    public int GetComValue() => comValue;
    public bool IsBowModeEnabled() => bowMode;
}
