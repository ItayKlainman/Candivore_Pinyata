using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject pinataPrefab;
    [SerializeField] private Transform pinataSpawnPoint;

    [SerializeField] private GameObject upgradeStore;
    
    [SerializeField] private float baseHP = 100f;
    [SerializeField] private float hpMultiplier = 1.5f;
    [SerializeField] private float baseTime = 10f;
    [SerializeField] private float timePerLevel = 2f;

    [SerializeField] private TextMeshProUGUI timerText;

    private GameObject currentPinata;
    private float currentTime;
    private float currentPinataHP;
    private int currentLevel = 1;
    private bool isLevelRunning = false;

    private void Awake()
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
        
        var hp = baseHP + (currentLevel * hpMultiplier);
        var timeLimit = baseTime + (currentLevel * timePerLevel);

        currentPinata = Instantiate(pinataPrefab, pinataSpawnPoint.position, Quaternion.identity);
        currentPinata.gameObject.SetActive(true);
        currentPinataHP = hp;
        currentTime = timeLimit;
        isLevelRunning = true;
        
        currentPinata.GetComponent<PinyataController>().Initialize(hp, OnPinataBroken);

        GameStateManager.Instance.SetGameState(GameState.Playing);
    }

    private void OnPinataBroken()
    {
        Debug.Log("Pinata defeated!");
        EndLevel();
    }

    private void EndLevel()
    {
        if (!isLevelRunning) return;

        isLevelRunning = false;
        Destroy(currentPinata);
        GameStateManager.Instance.SetGameState(GameState.Upgrading);
      //  upgradeStore.SetActive(true);
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
