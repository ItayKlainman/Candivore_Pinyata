using UnityEngine.Serialization;

[System.Serializable]
public class PlayerStats
{
// --- Live Stats (calculated from upgrade levels)
    public float damagePerSwipe = 10f;
    public float critChance = 0.1f;
    public int coinsPerHit = 1;
    public float critMultiplier = 2f;

// --- Player Progress
    public int totalCoins = 0;

// --- Upgrade Levels (saved/loaded)
    public int damageLevel = 1;
    public int critLevel = 1;
    public int coinLevel = 1;

// --- Upgrade Config Values 
    public float baseDamage = 10f;
    public float baseCritChance = 0.1f;
    public int baseCoins = 1;

    public float damageUpgradeStep = 2f;
    public float critUpgradeStep = 0.05f;
    public int coinUpgradeStep = 1;


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

    public void ApplyUpgradeLevels()
    {
        damagePerSwipe = baseDamage + (damageLevel - 1) * damageUpgradeStep;
        critChance = baseCritChance + (critLevel - 1) * critUpgradeStep;
        coinsPerHit = baseCoins + (coinLevel - 1) * coinUpgradeStep;
    }

}


public enum UpgradeType
{
    Damage,
    CritChance,
    CoinValue
}