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
            TryUpgrade(UpgradeType.Damage);
        });

        critButton.onClick.AddListener(() =>
        {
            AnimateButton(critButton);
            TryUpgrade(UpgradeType.CritChance);
        });

        coinButton.onClick.AddListener(() =>
        {
            AnimateButton(coinButton);
            TryUpgrade(UpgradeType.CoinValue);
        });

        continueButton.onClick.AddListener(() =>
        {
            AnimateButton(continueButton);
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
        PlayerStatsManager.Instance.ApplyUpgrade(type, cost);
        UpdateUI();
    }

    private void UpdateUI()
    {
        var stats = PlayerStatsManager.Instance.stats;

        coinsText.text = $"Coins: {stats.totalCoins}";
        damageCostText.text = $"Upgrade Damage ({stats.GetUpgradeCost(UpgradeType.Damage)})";
        critCostText.text = $"Upgrade Crit ({stats.GetUpgradeCost(UpgradeType.CritChance)})";
        coinCostText.text = $"Upgrade Coins ({stats.GetUpgradeCost(UpgradeType.CoinValue)})";

        currentDamageLevelText.SetText($"Lv.{stats.damageLevel}");
        currentCritLevelText.SetText($"Lv.{stats.critLevel}");
        CurrentCoinsBonusLevelText.SetText($"Lv.{stats.coinLevel}");
    }

    private void HandleContinue()
    {
        GameStateManager.Instance.SetGameState(GameState.Start);
    }

    private void AnimateButton(Button button)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.DOKill(); // Cancel any ongoing animation
        rect.DOScale(1.1f, 0.1f).SetLoops(2, LoopType.Yoyo);
    }
}
