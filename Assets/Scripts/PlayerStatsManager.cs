using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance;

    public PlayerStats stats = new PlayerStats();

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
    }

    public bool TrySpendCoins(int cost)
    {
        if (stats.totalCoins >= cost)
        {
            stats.totalCoins -= cost;
            return true;
        }
        return false;
    }

    public void ApplyUpgrade(UpgradeType type, int cost)
    {
        if (TrySpendCoins(cost))
        {
            stats.ApplyUpgrade(type);
            Debug.Log($"Upgrade applied: {type}");
        }
        else
        {
            Debug.Log("Not enough coins for upgrade");
        }
    }
}