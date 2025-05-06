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

    [Header("COINS UI")] [SerializeField] private CoinUIController coinUIController;

    [Header("HEALTH PACK SETTINGS")] [SerializeField]
    private GameObject healthPackPrefab;

    [SerializeField] private Transform[] healthPackSpawnPoints;
    [SerializeField] private float spawnMinDelay = 5f;
    [SerializeField] private float spawnMaxDelay = 10f;
    [SerializeField] private int maxHealthPacksPerLevel = 2;

    [Header("LEVEL CONFIG")] [SerializeField]
    private LevelConfigDatabase levelDatabase;

    [SerializeField] private Transform backgroundContainer;
    private GameObject currentBackgroundInstance;

    private Coroutine healthPackRoutine;

    private List<GameObject> healthPacks;
    private PinyataController currentPinataController;
    private float currentTime;
    private float currentPinataHP;
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
        Debug.Log($"Starting Level {GameStateManager.Instance.CurrentLevel}");

        isLevelRunning = true;
        coinUIController.gameObject.SetActive(true);

        var config = SetLevelData();

        InitializePinata(config.pinataHP);
        _swipeInput?.Initialize(currentPinataController);

        SetHealthPacks();

        SetTimer(config);

        GameStateManager.Instance.SetGameState(GameState.Playing);
    }

    private void SetTimer(LevelConfig config)
    {
        currentTime = config.timeLimit;
        InitHPBar(config.pinataHP);
    }

    private void SetHealthPacks()
    {
        if (healthPackRoutine != null)
            StopCoroutine(healthPackRoutine);
        healthPackRoutine = StartCoroutine(HealthPackSpawner());
    }


    private LevelConfig SetLevelData()
    {
        LevelConfig config = levelDatabase.GetLevel(GameStateManager.Instance.CurrentLevel - 1);

        var levelBonus = Mathf.FloorToInt(GameStateManager.Instance.CurrentLevel / 3f);
        PlayerStatsManager.Instance.stats.coinsPerHit += levelBonus;

        // Background
        if (currentBackgroundInstance != null)
            Destroy(currentBackgroundInstance);

        if (config.backgroundPrefab != null)
        {
            currentBackgroundInstance = Instantiate(config.backgroundPrefab, backgroundContainer);
            currentBackgroundInstance.transform.localPosition = Vector3.zero;
        }

        // NEW: Apply health pack timing
        spawnMinDelay = config.healthPackMinDelay;
        spawnMaxDelay = config.healthPackMaxDelay;

        return config;
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
}