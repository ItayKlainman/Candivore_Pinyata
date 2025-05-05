using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Transform pinataSpawnPoint;
    [SerializeField] private Rigidbody2D lastRopeSegmentRB;

    [SerializeField] private SwipeInput _swipeInput;
    
    [SerializeField] private GameObject pinataPrefab;
    [SerializeField] private GameObject upgradeStore;
    
    [SerializeField] private float baseHP = 100f;
    [SerializeField] private float hpMultiplier = 1.5f;
    [SerializeField] private float baseTime = 10f;
    [SerializeField] private float timePerLevel = 2f;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI _healthText;
    

    private PinyataController currentPinataController;
    private float currentTime;
    private float currentPinataHP;
    private int currentLevel = 1;
    private bool isLevelRunning = false;

    public void Initialize()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    private void Update()
    {
        if (isLevelRunning)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();

            if (currentTime <= 0)
            {
                EndLevel();
            }
        }
    }

    private void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Start)
        {
            StartLevel();
        }
    }

    public void StartLevel()
    {
        Debug.Log($"Starting Level {currentLevel}");
        
        var levelBonus = Mathf.FloorToInt(currentLevel / 3f); // every 3 levels = +1 coin
        PlayerStatsManager.Instance.stats.coinsPerHit += levelBonus;
        
        var pinataHP = baseHP + (currentLevel * hpMultiplier);
        var timeLimit = baseTime + (currentLevel * timePerLevel);
        
        InitializePinata(pinataHP);
        _swipeInput.Initialize(currentPinataController);
        
        currentTime = timeLimit;
        isLevelRunning = true;

        InitHPBar(pinataHP);

        GameStateManager.Instance.SetGameState(GameState.Playing);
    }

    private void InitializePinata(float hp)
    {
        var pinataGO =  Instantiate(pinataPrefab, pinataSpawnPoint.position, Quaternion.identity);
        pinataGO.transform.SetParent(pinataSpawnPoint.transform.parent);
        currentPinataHP = hp;

        currentPinataController = pinataGO.GetComponent<PinyataController>();
        currentPinataController.gameObject.GetComponent<HingeJoint2D>().connectedBody = lastRopeSegmentRB;
        
        currentPinataController.Initialize(hp, OnPinataBroken);
        currentPinataController.gameObject.SetActive(true);
        currentPinataController.OnPinyataHit += OnPinataControllerHit;
    }
    
    private void InitHPBar(float startHP)
    {
        healthBar.value = 1;
        _healthText.SetText($"{startHP}/{startHP}");
    }

    private void OnPinataControllerHit(float currentHp, float maxHp)
    {
        healthBar.value = currentHp /maxHp;
        _healthText.SetText($"{currentHp}/{maxHp}");
    }
    
    private void OnPinataBroken()
    {
        Debug.Log("Pinata defeated!");
        EndLevel();
    }

    private void EndLevel()
    {
        if (!isLevelRunning) return;
        
        currentPinataController.OnPinyataHit -= OnPinataControllerHit;
        
        isLevelRunning = false;
        Destroy(currentPinataController.gameObject);
        GameStateManager.Instance.SetGameState(GameState.Upgrading);
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(currentTime).ToString();
    }

    public void NextLevel()
    {
        currentLevel++;
        GameStateManager.Instance.SetGameState(GameState.Start);
    }
}
