using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; set; }
    [SerializeField] private SlowTimeSO slowTime;

    [Header("Start Scene")]
    public GameObject coverDome;
    public GameObject startUI;

    [Header("Level one")]
    public GameObject firstRoad;
    public GameObject firstActivator;


    [Header("Lose Screen")]
    public GameObject loseUI;
    public GameObject loseDome;
    [SerializeField] private TMPro.TextMeshProUGUI loseScoreText;

    [Header("Win Screen")]
    public GameObject winUI;
    public GameObject winDome;
    [SerializeField] private TMPro.TextMeshProUGUI winScoreText;
    private bool gameEnded = false;
    private void Update()
    {

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            Debug.Log("W key pressed");
            WinGame();
        }

        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            Debug.Log("L key pressed");
            LoseGame();
        }

    }


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void FinishStartUI()
    {
        startUI.SetActive(false);
        firstActivator.SetActive(true);
        firstRoad.SetActive(true);
    }

    public async void LoseGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        GameManager.Instance.player.GetComponent<StateController>()?.SetState(PlayerState.Shooting);
        slowTime.RaiseSlowTimeEnter(0f);

        // Player movement stop
        var rb = GameManager.Instance.player.GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = Vector3.zero;

        loseDome.SetActive(true);

        await UniTask.Delay(1000);

        if (loseScoreText != null)
        {
            loseScoreText.text = "Score: " + GameManager.Instance.player.GetComponent<Player>().Score;
        }

        //wait until the coverdome animation is done
        loseUI.SetActive(true);

    }


    public async void WinGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        GameManager.Instance.player.GetComponent<StateController>()?.SetState(PlayerState.Shooting);
        slowTime.RaiseSlowTimeEnter(0f);

        // Player movement stop
        var rb = GameManager.Instance.player.GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = Vector3.zero;

        winDome.SetActive(true);

        if (winScoreText != null)
        {
            winScoreText.text = "Score: " + GameManager.Instance.player.GetComponent<Player>().Score;
        }


        await UniTask.Delay(1000);
        //wait until the coverdome animation is done
        winUI.SetActive(true);
    }
}
