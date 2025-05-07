using System;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class CoinUIController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI coinText;
    public RectTransform icon;

    [Header("Floating Text")]
    [SerializeField] private GameObject floatingCoinTextPrefab;
    [SerializeField] private Canvas parentCanvas;

    private int currentCoins = 0;
    private bool initialized = false;

    void Start()
    {
        currentCoins = PlayerStatsManager.Instance.stats.totalCoins;
        coinText.text = currentCoins.ToString();
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
        int gained = newAmount - currentCoins;
        currentCoins = newAmount;

        coinText.text = currentCoins.ToString();
        icon.DOPunchScale(Vector3.one * 0.2f, 0.2f, 6, 1);

        // Only show floating text if already initialized AND gained coins in a valid way
        if (initialized && gained > 0 && gained < 100) // Prevent showing big diff from initial sync
        {
            ShowFloatingCoinText(gained);
        }

        initialized = true;
    }

    private void ShowFloatingCoinText(int amount)
    {
        var instance = ObjectPool.Instance.GetFromPool("CoinText", parentCanvas.transform.position, Quaternion.identity);
        instance.transform.SetParent(parentCanvas.transform, false);

        var rect = instance.GetComponent<RectTransform>();
        rect.position = icon.position + new Vector3(0, -80f, 0);

        var text = instance.GetComponent<TextMeshProUGUI>();
        var canvasGroup = instance.GetComponent<CanvasGroup>();

        text.text = $"+{amount}";
        rect.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        Sequence s = DOTween.Sequence();
        s.Append(rect.DOScale(1f, 0.15f).SetEase(Ease.OutBack));
        s.Join(canvasGroup.DOFade(1f, 0.1f));
        s.AppendInterval(0.4f);
        s.Append(canvasGroup.DOFade(0f, 0.2f));
        s.Join(rect.DOMoveY(rect.position.y - 40f, 0.2f));
        s.OnComplete(() => Destroy(instance));
    }
}
