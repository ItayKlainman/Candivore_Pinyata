using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UpgradeUIManager : MonoBehaviour
{
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private RectTransform upgradePanelRect;

    [SerializeField] private Button damageButton;
    [SerializeField] private Button critButton;
    [SerializeField] private Button coinButton;
    [SerializeField] private Button continueButton;

    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI damageCostText;
    [SerializeField] private TextMeshProUGUI critCostText;
    [SerializeField] private TextMeshProUGUI coinCostText;

    [SerializeField] private TextMeshProUGUI currentDamageLevelText;
    [SerializeField] private TextMeshProUGUI currentCritLevelText;
    [SerializeField] private TextMeshProUGUI CurrentCoinsBonusLevelText;

    private Vector3 panelHiddenScale = Vector3.zero;
    private Vector3 panelVisibleScale = Vector3.one;

    public void Initialize()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;

        damageButton.onClick.AddListener(() =>
        {
            AnimateButton(damageButton);
            FeedbackManager.Play("ButtonClick", FeedbackStrength.Light, 0.7f);
            TryUpgrade(UpgradeType.Damage);
        });

        critButton.onClick.AddListener(() =>
        {
            AnimateButton(critButton);
            FeedbackManager.Play("ButtonClick", FeedbackStrength.Light, 0.7f);
            TryUpgrade(UpgradeType.CritChance);
        });

        coinButton.onClick.AddListener(() =>
        {
            AnimateButton(coinButton);
            FeedbackManager.Play("ButtonClick", FeedbackStrength.Light, 0.7f);
            TryUpgrade(UpgradeType.CoinValue);
        });

        continueButton.onClick.AddListener(() =>
        {
            AnimateButton(continueButton);
            FeedbackManager.Play("ButtonClick", FeedbackStrength.Light, 0.7f);
            HandleContinue();
        });

        upgradePanelRect.localScale = panelHiddenScale;
        upgradePanel.SetActive(false);
    }

    private void OnDisable()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Upgrading)
        {
            upgradePanel.SetActive(true);
            upgradePanelRect.localScale = panelHiddenScale;
            upgradePanelRect.DOScale(panelVisibleScale, 0.4f).SetEase(Ease.OutBack);
            UpdateUI();
        }
        else if (newState != GameState.Upgrading && upgradePanel.activeSelf)
        {
            upgradePanelRect.DOScale(panelHiddenScale, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
            {
                upgradePanel.SetActive(false);
            });
        }
    }

    private void TryUpgrade(UpgradeType type)
    {
        var cost = PlayerStatsManager.Instance.stats.GetUpgradeCost(type);
        if (PlayerStatsManager.Instance.stats.totalCoins >= cost)
        {
            PlayerStatsManager.Instance.ApplyUpgrade(type, cost);
            FeedbackManager.Play("Upgrade", FeedbackStrength.Medium);
        }
        else
        {
            FeedbackManager.Play("Error", FeedbackStrength.Light);
        }

        UpdateUI();
    }
    
    private void UpdateUI()
    {
        var stats = PlayerStatsManager.Instance.stats;

        coinsText.text = $"Coins: {stats.totalCoins}";
        damageCostText.text = $"{stats.GetUpgradeCost(UpgradeType.Damage)}";
        critCostText.text = $"{stats.GetUpgradeCost(UpgradeType.CritChance)}";
        coinCostText.text = $"{stats.GetUpgradeCost(UpgradeType.CoinValue)}";

        currentDamageLevelText.SetText($"Lv.{stats.damageLevel}");
        currentCritLevelText.SetText($"Lv.{stats.critLevel}");
        CurrentCoinsBonusLevelText.SetText($"Lv.{stats.coinLevel}");
    }

    private void HandleContinue()
    {
        GameStateManager.Instance.SetGameState(GameState.Start);
        
        if (LevelManager.Instance != null && LevelManager.Instance.IsConfettiRunning())
        {
            LevelManager.Instance.StopConfetti();
        }
    }

    private void AnimateButton(Button button)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.DOKill(true);
        rect.localScale = Vector3.one;
        rect.DOScale(1.15f, 0.1f)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo);
    }

}
