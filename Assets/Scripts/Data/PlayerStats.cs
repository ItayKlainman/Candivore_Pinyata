using UnityEngine.Serialization;

[System.Serializable]
public class PlayerStats
{
    public float damagePerSwipe = 10f;
    public float critChance = 0.1f;
    public int coinsPerHit = 1; 
    public float critMultiplier = 2f;

    public int totalCoins = 0;
    
    public int damageLevel = 0;
    public int critLevel = 0;
    public int coinLevel = 0;

    public int GetUpgradeCost(UpgradeType type)
    {
        var baseCost = 5;

        var level = type switch
        {
            UpgradeType.Damage => damageLevel,
            UpgradeType.CritChance => critLevel,
            UpgradeType.CoinValue => coinLevel,
            _ => 0
        };

        return baseCost + (level * 5); // cost goes 5 → 10 → 15 ...
    }

    public void ApplyUpgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Damage:
                damagePerSwipe += 2f;
                damageLevel++;
                break;
            case UpgradeType.CritChance:
                critChance += 0.05f;
                critLevel++;
                break;
            case UpgradeType.CoinValue:
                coinsPerHit += 1;
                coinLevel++;
                break;
        }
    }
}


public enum UpgradeType
{
    Damage,
    CritChance,
    CoinValue
}