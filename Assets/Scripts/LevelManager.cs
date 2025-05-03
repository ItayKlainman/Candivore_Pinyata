using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject pinataPrefab;
    public Transform pinataSpawnPoint;
    public float baseHP = 100f;
    public float hpMultiplier = 1.5f;
    public float baseTime = 10f;
    public float timePerLevel = 2f;

    public TextMeshProUGUI timerText;

    private GameObject currentPinata;
    private float currentTime;
    private float currentPinataHP;
    private int currentLevel = 1;
    private bool isLevelRunning = false;

    private void OnEnable()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
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
        GameStateManager.Instance.SetGameState(GameState.Ended);
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
