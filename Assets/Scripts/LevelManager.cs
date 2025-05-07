using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    [SerializeField] private TextMeshProUGUI levelCounter;

    [Header("COINS UI")] [SerializeField] private CoinUIController coinUIController;

    [Header("Timer UI")] [SerializeField] private CanvasGroup timerCanvasGroup;

    [Header("HEALTH PACK SETTINGS")] [SerializeField]
    private GameObject healthPackPrefab;

    [SerializeField] private Transform[] healthPackSpawnPoints;
    [SerializeField] private float spawnMinDelay = 5f;
    [SerializeField] private float spawnMaxDelay = 10f;
    [SerializeField] private int maxHealthPacksPerLevel = 2;

    [Header("LEVEL CONFIG")] [SerializeField]
    private LevelConfigDatabase levelDatabase;

    [Header("POPUP SETTINGS")] [SerializeField]
    private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private float popupDuration = 1.5f;
    
    [Header("End Game")]
    [SerializeField] private GameObject endGamePopupPanel;
    [SerializeField] private CanvasGroup endGamePopupCanvasGroup;

    [SerializeField] private Transform backgroundContainer;
    private GameObject currentBackgroundInstance;

    private Coroutine healthPackRoutine;

    private List<GameObject> healthPacks;
    private PinyataController currentPinataController;
    private float currentTime;
    private float currentPinataHP;
    private bool isLevelRunning = false;

    public static LevelManager Instance { get; private set; }
    private List<GameObject> activeCoinParticles = new();

    private void Awake()
    {
        Instance = this;
    }

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

    private bool isStartingLevel = false;

    private void StartLevel()
    {
        if (isStartingLevel) return;
        isStartingLevel = true;


        coinUIController.gameObject.SetActive(true);
        levelCounter.SetText($"Level {GameStateManager.Instance.CurrentLevel}");

        var config = SetLevelData();
        InitializePinata(config.pinataHP);

        SetHealthPacks();
        InitHPBar(config.pinataHP);
        FeedbackManager.Play("LevelIntro", FeedbackStrength.None, 0.6f);
        StartCoroutine(ShowPopup($"Level {GameStateManager.Instance.CurrentLevel}", () => { BeginLevel(config); }));
    }

    private void BeginLevel(LevelConfig config)
    {
        _swipeInput?.Initialize(currentPinataController);
        isLevelRunning = true;
        SetTimer(config);
        ShowTimerUI();
        
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

        if (currentBackgroundInstance != null)
            Destroy(currentBackgroundInstance);

        if (config.backgroundPrefab != null)
        {
            currentBackgroundInstance = Instantiate(config.backgroundPrefab, backgroundContainer);
            currentBackgroundInstance.transform.localPosition = Vector3.zero;
        }

        spawnMinDelay = config.healthPackMinDelay;
        spawnMaxDelay = config.healthPackMaxDelay;
        maxHealthPacksPerLevel = config.MaxHealthPacks;

        return config;
    }

    private void HealPinata(float amount)
    {
        if (currentPinataController == null) return;

        currentPinataController.Heal(amount);
        //Debug.Log($"Pinata healed for {amount}!");
    }
    
    private void InitializePinata(float hp)
    {
        if (currentPinataController != null)
        {
            Destroy(currentPinataController.gameObject);
            currentPinataController = null;
        }

        var pinataGO = Instantiate(pinataPrefab, pinataSpawnPoint.position, Quaternion.identity);
        pinataGO.transform.SetParent(pinataSpawnPoint.transform.parent);
        currentPinataHP = hp;

        currentPinataController = pinataGO.GetComponent<PinyataController>();
        currentPinataController.GetComponent<HingeJoint2D>().connectedBody = lastRopeSegmentRB;

        currentPinataController.Initialize(hp, OnPinataBroken);
        currentPinataController.gameObject.SetActive(true);
        currentPinataController.OnPinyataHit += OnPinataControllerHit;
    }


    private IEnumerator HealthPackSpawner()
    {
        var spawnedCount = 0;
        healthPacks = new List<GameObject>();

        while (isStartingLevel && spawnedCount < maxHealthPacksPerLevel)
        {
            var delay = Random.Range(spawnMinDelay, spawnMaxDelay);
            yield return new WaitForSeconds(delay);

            if (!isStartingLevel) yield break;

            var index = Random.Range(0, healthPackSpawnPoints.Length);
            var spawnPoint = healthPackSpawnPoints[index];

            yield return StartCoroutine(SpawnHealthPackNextFrame(spawnPoint));
            spawnedCount++;
        }
    }

    private IEnumerator SpawnHealthPackNextFrame(Transform spawnPoint)
    {
        yield return null;

        if (!isStartingLevel) yield break;

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
        var currentHPcClamp = Mathf.Clamp(currentHp, 0, maxHp);
        _healthText.SetText($"{currentHPcClamp}/{maxHp}");
    }

    private void OnPinataBroken()
    {
        EndLevel();
    }

    private void EndLevel()
    {
        isStartingLevel = false;
        if (!isLevelRunning) return;

        if (healthPackRoutine != null)
        {
            StopCoroutine(healthPackRoutine);
            healthPackRoutine = null;
        }

        HideTimerUI();

        foreach (var healthPack in healthPacks)
        {
            ObjectPool.Instance.ReturnToPool("HealthPack", healthPack);
        }

        foreach (var psGO in activeCoinParticles)
        {
            ObjectPool.Instance.ReturnToPool("Coin", psGO);
        }

        activeCoinParticles.Clear();

        healthPacks.Clear();

        currentPinataController.OnPinyataHit -= OnPinataControllerHit;

        isLevelRunning = false;
        
        string resultText = currentTime <= 0 ? "OUT OF TIME" : "YOU WIN!";
        bool won = currentTime > 0;

        if (won)
        {
            FeedbackManager.Play("Tada", FeedbackStrength.None, 0.5f);
        }
        else
        {
            FeedbackManager.Play("Failed", FeedbackStrength.Medium, 0.5f);
        }

        StartCoroutine(ShowPopup(resultText, () =>
        {
            if (won)
            {
                GameStateManager.Instance.CurrentLevel++;
                PlayerStatsManager.Instance.SaveProgress();
                confettiRoutine = StartCoroutine(SpawnConfettiDuringUpgrade());
            }
            
            if (GameStateManager.Instance.CurrentLevel > 12)
            {
                StartCoroutine(ShowEndGamePopup());
                return;
            }
            
            GameStateManager.Instance.SetGameState(GameState.Upgrading);
        }));
    }

    private void ShowTimerUI()
    {
        timerCanvasGroup.alpha = 0f;
        timerText.rectTransform.localScale = Vector3.zero;

        timerCanvasGroup.DOFade(1f, 0.3f);
        timerText.rectTransform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
    }

    private void HideTimerUI()
    {
        timerCanvasGroup.DOFade(0f, 0.2f);
        timerText.rectTransform.DOScale(0f, 0.3f).SetEase(Ease.InBack);
    }

    private float lastTickSecond = -1f;

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.CeilToInt(currentTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";

        if (totalSeconds <= 5)
        {
            timerText.color = Color.red;

            if (!DOTween.IsTweening(timerText.transform))
            {
                timerText.transform.DOKill();
                timerText.transform
                    .DOScale(1.3f, 0.3f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.OutBack);
            }

            if (!Mathf.Approximately(lastTickSecond, totalSeconds))
            {
                lastTickSecond = totalSeconds;
                
                CameraShakeManager.Instance?.Shake(0.1f, 0.2f); 
                FeedbackManager.Play("TimerTick", FeedbackStrength.Light, 0.7f);
            }
        }
        else
        {
            timerText.color = Color.white;
            timerText.transform.localScale = Vector3.one;
            lastTickSecond = -1f;
        }
    }


    private IEnumerator ShowPopup(string message, System.Action onComplete = null)
    {
        popupText.text = message;
        popupCanvasGroup.alpha = 0;
        popupPanel.SetActive(true);
        popupCanvasGroup.DOFade(1f, 0.3f);
        popupPanel.transform.localScale = Vector3.zero;
        popupPanel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(popupDuration);

        popupCanvasGroup.DOFade(0f, 0.2f);
        popupPanel.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                popupPanel.SetActive(false);
                onComplete?.Invoke();
            });
    }
    
    private IEnumerator ShowEndGamePopup()
    {
        endGamePopupPanel.SetActive(true);
        endGamePopupCanvasGroup.alpha = 0;
        endGamePopupCanvasGroup.DOFade(1f, 0.5f);
        endGamePopupCanvasGroup.interactable = true;
        yield return null;
    }
    
    public void SpawnCoins(int amount, Vector3 pos)
    {
        for (int i = 0; i < amount; i++)
        {
            var coinGO = ObjectPool.Instance.GetFromPool("Coin", pos, Quaternion.identity);
            var ps = coinGO.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play();
                activeCoinParticles.Add(coinGO);
                StartCoroutine(ReturnParticleToPoolAfter(ps, "Coin",
                    ps.main.duration + ps.main.startLifetime.constantMax));
            }
        }
    }

    private IEnumerator ReturnParticleToPoolAfter(ParticleSystem ps, string poolKey, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (ps != null && ps.gameObject.activeInHierarchy)
        {
            ObjectPool.Instance.ReturnToPool(poolKey, ps.gameObject);
        }
    }
    
    public void PlayConfettiBurst(Vector3 center)
    {
        var burstCount = 6; 
        var radius = 3f;

        for (int i = 0; i < burstCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * radius;
            Vector3 spawnPos = center + new Vector3(offset.x, offset.y, 0f);

            var confetti = ObjectPool.Instance.GetFromPool("Confetti", spawnPos, Quaternion.identity);
            FeedbackManager.Play("Pop",FeedbackStrength.None, 0.5f, 0.3f);
            var ps = confetti.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play();

                float delay = ps.main.duration + ps.main.startLifetime.constantMax;
                StartCoroutine(ReturnParticleToPoolAfter(ps, "Confetti", delay));
            }
        }
    }


    private Coroutine confettiRoutine;

    private IEnumerator SpawnConfettiDuringUpgrade()
    {
        var upgradeDuration = 999f;
        var screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));

            Vector3 spawnPos = new Vector3(
                Random.Range(-screenBounds.x, screenBounds.x),
                Random.Range(-screenBounds.y, screenBounds.y),
                0f
            );

            var confetti = ObjectPool.Instance.GetFromPool("Confetti", spawnPos, Quaternion.identity);
            var ps = confetti.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play();
                FeedbackManager.Play("Pop",FeedbackStrength.None, 0.5f, 0.3f);
                float delay = ps.main.duration + ps.main.startLifetime.constantMax;
                StartCoroutine(ReturnParticleToPoolAfter(ps, "Confetti", delay));
            }
        }
    }
    
    public void StopConfetti()
    {
        if (confettiRoutine != null)
        {
            StopCoroutine(confettiRoutine);
            confettiRoutine = null;
        }
    }

    public bool IsConfettiRunning() => confettiRoutine != null;

}