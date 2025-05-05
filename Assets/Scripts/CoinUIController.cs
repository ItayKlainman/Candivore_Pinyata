using System;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class CoinUIController : MonoBehaviour
{
    public TextMeshProUGUI coinText;
    public RectTransform icon;
    
    private int currentCoins = 0;

    void Start()
    {
        UpdateCoins(PlayerStatsManager.Instance.stats.totalCoins);
    }

    private void OnEnable()
    {
        PlayerStatsManager.OnCoinsChanged += UpdateCoins;
    }

    private void OnDisable()
    {
        PlayerStatsManager.OnCoinsChanged -= UpdateCoins;
    }

    public void UpdateCoins(int newAmount)
    {
        currentCoins = newAmount;
        coinText.text = currentCoins.ToString();

        icon.DOPunchScale(Vector3.one * 0.2f, 0.2f, 6, 1);
    }
}