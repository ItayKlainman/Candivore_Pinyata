using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUIManager : MonoBehaviour
{
    [SerializeField] private GameObject upgradePanel;
    
    [SerializeField] private  Button damageButton;
    [SerializeField] private  Button critButton;
    [SerializeField] private  Button coinButton;
    [SerializeField] private Button continueButton;
    
    [SerializeField] private  TextMeshProUGUI coinsText;
    [SerializeField] private  TextMeshProUGUI damageCostText;
    [SerializeField] private  TextMeshProUGUI critCostText;
    [SerializeField] private  TextMeshProUGUI coinCostText;

    private void OnEnable()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;

        damageButton.onClick.AddListener(() => TryUpgrade(UpgradeType.Damage));
        critButton.onClick.AddListener(() => TryUpgrade(UpgradeType.CritChance));
        coinButton.onClick.AddListener(() => TryUpgrade(UpgradeType.CoinValue));
        continueButton.onClick.AddListener(HandleContinue);

    }

    private void OnDisable()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState newState)
    {
        upgradePanel.SetActive(newState == GameState.Upgrading);

        if (newState == GameState.Upgrading)
            UpdateUI();
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
        damageCostText.text = $"Upgrade Damage ({stats.GetUpgradeCost(UpgradeType.Damage)}c)";
        critCostText.text = $"Upgrade Crit ({stats.GetUpgradeCost(UpgradeType.CritChance)}c)";
        coinCostText.text = $"Upgrade Coins ({stats.GetUpgradeCost(UpgradeType.CoinValue)}c)";
    }

    private void HandleContinue()
    {
        GameStateManager.Instance.SetGameState(GameState.Start);
        upgradePanel.SetActive(false);
    }
}