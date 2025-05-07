using System;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance;

    public PlayerStats stats = new PlayerStats();

    public static event Action<int> OnCoinsChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoins(int amount)
    {
        stats.totalCoins += amount;
        OnCoinsChanged?.Invoke(stats.totalCoins);
    }

    public bool TrySpendCoins(int cost)
    {
        if (stats.totalCoins >= cost)
        {
            stats.totalCoins -= cost;
            OnCoinsChanged?.Invoke(stats.totalCoins);
            return true;
        }

        return false;
    }

    public void ApplyUpgrade(UpgradeType type, int cost)
    {
        if (TrySpendCoins(cost))
        {
            stats.ApplyUpgrade(type);
            //Debug.Log($"Upgrade applied: {type}");
            SaveProgress();

        }
        else
        {
            //Debug.Log("Not enough coins for upgrade");
        }
    }

    public void SaveProgress()
    {
        PlayerPrefs.SetInt("Coins", stats.totalCoins);
        PlayerPrefs.SetInt("DamageLevel", stats.damageLevel);
        PlayerPrefs.SetInt("CritLevel", stats.critLevel);
        PlayerPrefs.SetInt("CoinLevel", stats.coinLevel);
        PlayerPrefs.SetInt("CurrentLevel", GameStateManager.Instance.CurrentLevel);

        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        stats.totalCoins = PlayerPrefs.GetInt("Coins", 0);
        stats.damageLevel = PlayerPrefs.GetInt("DamageLevel", 1);
        stats.critLevel = PlayerPrefs.GetInt("CritLevel", 1);
        stats.coinLevel = PlayerPrefs.GetInt("CoinLevel", 1);
        GameStateManager.Instance.CurrentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        stats.ApplyUpgradeLevels();
    }
    
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("Coins");
        PlayerPrefs.DeleteKey("DamageLevel");
        PlayerPrefs.DeleteKey("CritLevel");
        PlayerPrefs.DeleteKey("CoinLevel");
        PlayerPrefs.DeleteKey("CurrentLevel");

        stats.totalCoins = 0;
        stats.damageLevel = 1;
        stats.critLevel = 1;
        stats.coinLevel = 1;

        stats.ApplyUpgradeLevels();
        //Debug.Log("Player progress has been reset.");
    }

}