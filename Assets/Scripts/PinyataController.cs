using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class PinyataController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    
    [SerializeField] private float forceMultiplier = 10f;
    [SerializeField] private float maxHealth = 100f;
    
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
            // show crit FX here
        }

        var force = swipeStrength * 0.01f * forceMultiplier;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        PlayerStatsManager.Instance.AddCoins(stats.coinsPerHit);

        TakeDamage(damage, direction);
    }

    private void TakeDamage(float amount, Vector2 direction)
    {
        currentHealth -= amount;
        
        OnPinyataHit?.Invoke(currentHealth, maxHealth);
        
        // TODO: Play hit FX, sound, shake, squash/stretch here
        
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
        onBrokenCallback?.Invoke();
        
        // TODO: Play explosion, give rewards, transition to next level
    }
}