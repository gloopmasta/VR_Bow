using Cysharp.Threading.Tasks;
using UnityEngine;

public class NullPlayer : MonoBehaviour, IPlayerRuntime
{
    [Header("Player Data")]
    [SerializeField] private PlayerDataSO data;
    [SerializeField] private ArrowHitEventsSO arrowHitEvents;
    [SerializeField] private LevelEventsSO levelEvents;

    [Header("Stats")]
    [SerializeField] private int hp;
    [SerializeField] private int arrowCount;
    [SerializeField] private float slowTime;
    [SerializeField] private int score;
    [SerializeField] private bool isDashing;

    [Header("Respawn")]
    public Vector3 respawnPosition = Vector3.zero;
    public Vector3 respawnRotation = Vector3.zero;

    [Header("Feedback")]
    [SerializeField] private AudioCue takeDamageCue;

    public int Hp
    {
        get { return hp; }
        set { hp = Mathf.Min(value, data.MaxHp); }
    }

    public int ArrowCount
    {
        get { return arrowCount; }
        set { arrowCount = Mathf.Clamp(value, 0, data.MaxArrowCount); }
    }

    public float SlowTime
    {
        get { return slowTime; }
        set { slowTime = value; }
    }

    public int Score
    {
        get { return score; }
        set { score = value; }
    }

    public bool IsDashing
    {
        get { return isDashing; }
        set { isDashing = value; }
    }

    public Vector3 RespawnPosition
    {
        get { return respawnPosition; }
        set { respawnPosition = value; }
    }

    public Vector3 RespawnRotation
    {
        get { return respawnRotation; }
        set { respawnRotation = value; }
    }

    private void OnEnable()
    {
        arrowHitEvents.OnArrowHitEnemy += IncreaseScore;
        levelEvents.OnLevelOneStart += OnLevelStart;
        if (GameManager.Instance != null) GameManager.Instance.SetPlayer(gameObject);
    }

    private void Start()
    {
        ResetStats();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayer(gameObject);
        }
    }

    private void OnDisable()
    {
        arrowHitEvents.OnArrowHitEnemy -= IncreaseScore;
        levelEvents.OnLevelOneStart -= OnLevelStart;
        if (GameManager.Instance != null && GameManager.Instance.player == gameObject)
        {
            GameManager.Instance.RemovePlayer();
        }
    }

    private void OnLevelStart()
    {
        
    }

    public void ResetStats()
    {
        hp = data.MaxHp;
        arrowCount = data.MaxArrowCount;
        slowTime = 4f;
        score = 0;
        isDashing = false;
    }

    public async UniTaskVoid Respawn()
    {
        var ui = GetComponent<PlayerUIManager>();
        await ui.FadeToBlackAsync();
        MoveTo(respawnPosition, respawnRotation);
        await ui.FadeFromBlackAsync();
    }

    public void ResetPosition()
    {
        respawnPosition = Vector3.zero;
        respawnRotation = Vector3.zero;
        MoveTo(Vector3.zero, Vector3.zero);
    }

    private void MoveTo(Vector3 position, Vector3 rotation)
    {
        var rb = GetComponent<Rigidbody>();
        Quaternion targetRotation = Quaternion.Euler(rotation);
        transform.SetPositionAndRotation(position, targetRotation);
        rb.position = position;
        rb.rotation = targetRotation;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        var driveControls = GetComponent<DriveControls>();
        if (driveControls != null) driveControls.currentVelocity = Vector3.zero;
    }

    public void IncreaseScore(int amount)
    {
        score += amount;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Powerup>(out var powerup))
        {
            powerup.Collect(gameObject);
        }
    }

}
