
using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class PinyataController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float forceMultiplier = 10f;
    [SerializeField] private float maxHealth = 100f;

    [Header("Coin Drop")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform coinSpawnPoint;
    [SerializeField] private int coinsOnHit = 1;
    [SerializeField] private int coinsOnBreak = 3;
    [SerializeField] private Vector2 coinScatterForce = new Vector2(1f, 2f);

    public delegate void PinyataHit(float currentHp, float maxHp);
    public event PinyataHit OnPinyataHit;

    private float currentHealth;
    public Action onBrokenCallback;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void Initialize(float maxHP, Action onBroken)
    {
        maxHealth = maxHP;
        currentHealth = maxHP;
        onBrokenCallback = onBroken;
    }

    public void OnSwipe(Vector2 direction, float swipeStrength)
    {
        var stats = PlayerStatsManager.Instance.stats;

        var damage = stats.damagePerSwipe;
        var isCrit = Random.value < stats.critChance;

        if (isCrit)
        {
            damage *= stats.critMultiplier;
            FeedbackManager.Play("Crit", FeedbackStrength.Medium, 1f, 0.3f);
        }
        else
        {
            FeedbackManager.Play("Hit", FeedbackStrength.Light, 0.8f, 0.3f);
        }

        ShowTextEffect(damage, isCrit);

        var force = swipeStrength * 0.01f * forceMultiplier;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        PlayerStatsManager.Instance.AddCoins(stats.coinsPerHit); 
        LevelManager.Instance.SpawnCoins(1, transform.position);
        
        TakeDamage(damage, direction);
    }
    
    private void ShowTextEffect(float damage, bool isCrit)
    {
        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 1f, 0f);
        var floatText = ObjectPool.Instance.GetFromPool("Text", spawnPos, Quaternion.identity);
        floatText.GetComponent<FloatingText>()?.Show(damage.ToString("F0"), isCrit ? Color.red : Color.white,
            spawnPos
        );
    }

    private void TakeDamage(float amount, Vector2 direction)
    {
        currentHealth -= amount;

        OnPinyataHit?.Invoke(currentHealth, maxHealth);
        PlayHitEffect(direction);

        if (currentHealth <= 0)
        {
            BreakPinata();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnPinyataHit?.Invoke(currentHealth, maxHealth);
    }

    private void PlayHitEffect(Vector2 dir)
    {
        transform.DOComplete();
        transform.DOPunchScale(dir * 0.2f, 0.2f);
    }

    private void BreakPinata()
    {
        Debug.Log("PINATA BROKEN!");
        FeedbackManager.Play("Break", FeedbackStrength.Heavy,0.5f);
        LevelManager.Instance.SpawnCoins(coinsOnBreak, transform.position);
        LevelManager.Instance?.PlayConfettiBurst(transform.position);
        
        onBrokenCallback?.Invoke();
    }
}
