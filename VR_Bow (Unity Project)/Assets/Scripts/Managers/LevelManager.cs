using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; set; }
    [SerializeField] LevelEventsSO levelEvents;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    [SerializeField] private SlowTimeSO slowTime;

    

    private void OnEnable()
    {
        levelEvents.OnLevelOneStart += () => FinishStartUI().Forget();
        levelEvents.OnLevelOneLose += () => LoseGame();
        levelEvents.OnLevelOneWin += () => WinGame();
        levelEvents.OnLevelOneRestart += () => RestartLevel();
    }

    [Header("Start Scene")]
    public GameObject coverDome;
    public GameObject startUI;

    [Header("Level one")]
    [SerializeField] private GameObject mapPrefab;
    private GameObject currentMap;

    private GameObject firstRoad;
    private GameObject firstActivator;
    private GameObject map;

    [Header("Start Screen")]
    public GameObject startScreenUI;


    [Header("Lose Screen")]
    public GameObject loseUI;
    public GameObject loseDome;
    [SerializeField] private TMPro.TextMeshProUGUI loseScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI loseScoreText2;

    [Header("Win Screen")]
    public GameObject winUI;
    public GameObject winDome;
    [SerializeField] private TMPro.TextMeshProUGUI winScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI winScoreText2;

    private bool gameEnded = false;

    private void Start()
    {
        currentMap = Instantiate(mapPrefab);
        UpdateLevelReferences(currentMap);
    }


    public async UniTaskVoid FinishStartUI()
    {
        startScreenUI.GetComponent<Animator>().CrossFade("StartScreenFadeOut", 0f);
        await UniTask.WaitForSeconds(1);
        startUI.SetActive(false);
        firstActivator.SetActive(true);
        firstRoad.SetActive(true);
    }

    public void EnableStartUI()
    {
        startUI.SetActive(true);
        startScreenUI.GetComponent<Animator>().CrossFade("StartScreenFadeIn", 0f);
    }

    public async void LoseGame()
    {
        Debug.Log("Lost");
        if (gameEnded) return;
        gameEnded = true;

        GameManager.Instance.player.GetComponent<StateController>()?.SetState(PlayerState.Shooting);
        slowTime.RaiseSlowTimeEnter(0f);


        loseDome.SetActive(true);

        await UniTask.Delay(1000);

        if (loseScoreText != null)
        {
            loseScoreText.text = "Score: " + GameManager.Instance.player.GetComponent<Player>().Score;
            loseScoreText2.text = "Score: " + GameManager.Instance.player.GetComponent<Player>().Score;
        }

        //wait until the coverdome animation is done
        loseUI.SetActive(true);

    }


    public void WinGame()
    {
        Debug.Log("Player won game");
        if (gameEnded) return;
        gameEnded = true;

        //slowTime.RaiseSlowTimeEnter(0f); //SHOULD ALREADY BE IN SLOWTIME

        winDome.SetActive(true);

        if (winScoreText != null)
        {
            winScoreText.text = "Score: " + GameManager.Instance.player.GetComponent<Player>().Score;
            winScoreText2.text = "Score: " + GameManager.Instance.player.GetComponent<Player>().Score;
        }

        //await UniTask.Delay(1000);
        //wait until the coverdome animation is done
        winUI.SetActive(true);
    }

    public async void RestartLevel()
    {
        Debug.Log("Player pressed a restart button");
        var uiManager = GameManager.Instance.player.GetComponent<PlayerUIManager>();
        var playerScript = GameManager.Instance.player.GetComponent<Player>();

        await uiManager.FadeToBlackAsync(); // fade to black

        // Destroy the old map
        if (currentMap != null) Destroy(currentMap);

        gameEnded = false;

        //Vector3 targetRotation = new Vector3(0f, Camera.main.transform.rotation.y, 0f); // get camera y rotation

        //currentMap.transform.rotation = Quaternion.Euler(targetRotation);
        //startScreenUI.transform.rotation = currentMap.transform.rotation;

        winDome?.SetActive(false);
        winUI?.SetActive(false);
        loseUI?.SetActive(false);
        slowTime.RaiseSlowTimeExit();

        // Instantiate new one
        currentMap = Instantiate(mapPrefab);
        UpdateLevelReferences(currentMap);

        GameManager.Instance.player.GetComponent<DriveControls>().enabled = false; //disable drive controls
        //GameManager.Instance.player.GetComponent<OffRoadTracker>().enabled = false; //disable drive controls
        // Reset player
        playerScript.ResetPosition();
        playerScript.ResetStats();
        GameManager.Instance.player.GetComponent<Tutorial>().RestartTutorial();


        await uiManager.FadeFromBlackAsync(); // fade from black
        winDome?.SetActive(false);

        EnableStartUI();
    }


    private void UpdateLevelReferences(GameObject newMap)
    {
        map = newMap;
        var mapRoot = currentMap.GetComponent<MapRoot>();
        firstRoad = mapRoot.firstRoad;
        firstActivator = mapRoot.firstActivator;


        // Optionally: Add error logging
        if (firstRoad == null) Debug.LogError("FirstRoad not found in new map.");
        if (firstActivator == null) Debug.LogError("FirstActivator not found in new map.");
    }

}
