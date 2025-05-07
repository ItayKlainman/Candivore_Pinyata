
using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class PinyataController : MonoBehaviour
{
    private static readonly int Hit = Animator.StringToHash("Hit");
    private static readonly int IsBroken = Animator.StringToHash("IsBroken");

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float forceMultiplier = 10f;
    [SerializeField] private float maxHealth = 100f;
    
    [SerializeField] private Animator animator;

    [SerializeField] private Transform pinataParts;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Coin Drop")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform coinSpawnPoint;
    [SerializeField] private int coinsOnHit = 1;
    [SerializeField] private int coinsOnBreak = 3;
    [SerializeField] private Vector2 coinScatterForce = new Vector2(1f, 2f);
    
    [Header("Pinata Parts")]
    [SerializeField] private GameObject[] brokenPartsPrefabs; 
    [SerializeField] private Transform partSpawnPoint;
    [SerializeField] private float partForce = 3f;
    [SerializeField] private float partTorque = 20f;
    [SerializeField] private float partLifetime = 2f;
    
    [Header("Hit Sounds")]
    [SerializeField] private string[] randomHitSFXNames;

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
            FeedbackManager.Play("Crit", FeedbackStrength.Medium, 1f, 0.4f);
            CameraShakeManager.Instance.Shake(0.25f, 0.2f);
        }
        else
        {
            FeedbackManager.Play("Hit", FeedbackStrength.Medium, 1f, 0.4f);
            CameraShakeManager.Instance.Shake(0.1f, 0.2f);
        }
        
        if (randomHitSFXNames != null && randomHitSFXNames.Length > 0)
        {
            string clipName = randomHitSFXNames[Random.Range(0, randomHitSFXNames.Length)];
            FeedbackManager.Play(clipName, FeedbackStrength.Light, 0.6f, 0.3f);
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
        animator.SetTrigger(Hit);
        
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
        //Debug.Log("PINATA BROKEN!");
        FeedbackManager.Play("Break", FeedbackStrength.Heavy, 0.5f);
        LevelManager.Instance.SpawnCoins(coinsOnBreak, transform.position);
        LevelManager.Instance?.PlayConfettiBurst(transform.position);

        // Play breaking animation
        animator.SetBool("IsBroken", true);

        // Start coroutine to handle part spawning & cleanup
        StartCoroutine(HandlePinataBreakEffects());
    }

    private IEnumerator HandlePinataBreakEffects()
    {
        Sequence breakSequence = DOTween.Sequence();
        breakSequence.Join(transform.DOScale(0f, 0.2f).SetEase(Ease.InBack));

        if (_spriteRenderer != null)
        {
            breakSequence.Join(_spriteRenderer.DOFade(0f, 0.2f));
        }

        breakSequence.OnComplete(() =>
        {
            SpawnBrokenParts();
            Destroy(gameObject);
            onBrokenCallback?.Invoke();
        });

        yield return breakSequence.WaitForCompletion();
        
        yield return new WaitForSeconds(1f);

        Destroy(gameObject);
        onBrokenCallback?.Invoke();
    }

    private void SpawnBrokenParts()
    {
        foreach (var partPrefab in brokenPartsPrefabs)
        {
            Vector3 offset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0f);
            Vector3 spawnPos = partSpawnPoint.position + offset;

            GameObject part = Instantiate(partPrefab, spawnPos, Quaternion.identity);

            Rigidbody2D rb = part.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomForce = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f)) * partForce;
                float randomTorque = Random.Range(-partTorque, partTorque);

                rb.AddForce(randomForce, ForceMode2D.Impulse);
                rb.AddTorque(randomTorque, ForceMode2D.Impulse);
            }

            Destroy(part, partLifetime); 
        }
    }


}
