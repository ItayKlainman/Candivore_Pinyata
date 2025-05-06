using System.Collections;
using System.Collections.Generic;
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
    
    [Header("COINS UI")] [SerializeField]
    private CoinUIController coinUIController;
    
    [Header("HEALTH PACK SETTINGS")]
    [SerializeField] private GameObject healthPackPrefab;
    [SerializeField] private Transform[] healthPackSpawnPoints;
    [SerializeField] private float spawnMinDelay = 5f;
    [SerializeField] private float spawnMaxDelay = 10f;
    [SerializeField] private int maxHealthPacksPerLevel = 2;

    private Coroutine healthPackRoutine;

    private List<GameObject> healthPacks;
    private PinyataController currentPinataController;
    private float currentTime;
    private float currentPinataHP;
    private int currentLevel = 1;
    private bool isLevelRunning = false;

    public void Initialize()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
        HealthPack.OnHealthPackTouched += HealPinata;
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        HealthPack.OnHealthPackTouched -= HealPinata;
    }

    private void Update()
    {
        if (isLevelRunning)
        {
            currentTime = Mathf.Max(0, currentTime - Time.deltaTime);
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

    private void StartLevel()
    {
        Debug.Log($"Starting Level {currentLevel}");

        isLevelRunning = true;
        var levelBonus = Mathf.FloorToInt(currentLevel / 3f); // every 3 levels = +1 coin
        PlayerStatsManager.Instance.stats.coinsPerHit += levelBonus;
        coinUIController.gameObject.SetActive(true);

        var pinataHP = baseHP + (currentLevel * hpMultiplier);
        var timeLimit = baseTime + (currentLevel * timePerLevel);

        InitializePinata(pinataHP);
        _swipeInput?.Initialize(currentPinataController);
        
        if (healthPackRoutine != null)
            StopCoroutine(healthPackRoutine);

        healthPackRoutine = StartCoroutine(HealthPackSpawner());
        
        currentTime = timeLimit;

        InitHPBar(pinataHP);

        GameStateManager.Instance.SetGameState(GameState.Playing);
    }
    
    private void HealPinata(float amount)
    {
        if (currentPinataController == null) return;

        currentPinataController.Heal(amount);
        Debug.Log($"Pinata healed for {amount}!");
    }
    
    private void InitializePinata(float hp)
    {
        var pinataGO = Instantiate(pinataPrefab, pinataSpawnPoint.position, Quaternion.identity);
        pinataGO.transform.SetParent(pinataSpawnPoint.transform.parent);
        currentPinataHP = hp;

        currentPinataController = pinataGO.GetComponent<PinyataController>();
        currentPinataController.gameObject.GetComponent<HingeJoint2D>().connectedBody = lastRopeSegmentRB;

        currentPinataController.Initialize(hp, OnPinataBroken);
        currentPinataController.gameObject.SetActive(true);
        currentPinataController.OnPinyataHit += OnPinataControllerHit;
    }
    
    private IEnumerator HealthPackSpawner()
    {
        var spawnedCount = 0;
        healthPacks = new List<GameObject>();

        while (isLevelRunning && spawnedCount < maxHealthPacksPerLevel)
        {
            var delay = Random.Range(spawnMinDelay, spawnMaxDelay);
            yield return new WaitForSeconds(delay);

            if (!isLevelRunning) yield break;

            var index = Random.Range(0, healthPackSpawnPoints.Length);
            var spawnPoint = healthPackSpawnPoints[index];
            
            yield return StartCoroutine(SpawnHealthPackNextFrame(spawnPoint));
            spawnedCount++;
        }
    }

    private IEnumerator SpawnHealthPackNextFrame(Transform spawnPoint)
    {
        yield return null;

        if (!isLevelRunning) yield break;
        
        var pack = ObjectPool.Instance.GetFromPool("HealthPack", spawnPoint.position, Quaternion.identity);
        pack.transform.SetParent(spawnPoint);

        healthPacks.Add(pack);
    }

    
    private void InitHPBar(float startHP)
    {
        healthBar.value = 1;
        _healthText.SetText($"{startHP}/{startHP}");
    }

    private void OnPinataControllerHit(float currentHp, float maxHp)
    {
        healthBar.value = currentHp / maxHp;
        _healthText.SetText($"{currentHp}/{maxHp}");
    }

    private void OnPinataBroken()
    {
        EndLevel();
    }

    private void EndLevel()
    {
        if (!isLevelRunning) return;
        
        GameStateManager.Instance.CurrentLevel++;
        PlayerStatsManager.Instance.SaveProgress();

        if (healthPackRoutine != null)
        {
            StopCoroutine(healthPackRoutine);
            healthPackRoutine = null;
        }

        foreach (var healthPack in healthPacks)
        {
            ObjectPool.Instance.ReturnToPool("HealthPack", healthPack);
        }
        healthPacks.Clear();
        
        currentPinataController.OnPinyataHit -= OnPinataControllerHit;

        isLevelRunning = false;
        Destroy(currentPinataController.gameObject);
        
        GameStateManager.Instance.SetGameState(GameState.Upgrading);
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.CeilToInt(currentTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void NextLevel()
    {
        currentLevel++;
        GameStateManager.Instance.SetGameState(GameState.Start);
    }
}
