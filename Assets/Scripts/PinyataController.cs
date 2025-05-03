using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PinyataController : MonoBehaviour
{
    public Rigidbody2D rb;
    [SerializeField] private float forceMultiplier = 10f;
    [SerializeField] private Slider healthBar;
    
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void OnSwipe(Vector2 direction, float swipeStrength)
    {
        var force = swipeStrength * 0.01f * forceMultiplier;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        var damage = Mathf.Clamp(force, 5f, 25f); // Optional: scale damage based on swipe
        TakeDamage(damage);
    }

    private void TakeDamage(float amount)
    {
        currentHealth -= amount;
        healthBar.value = currentHealth / maxHealth;
        
        if (currentHealth <= 0)
            BreakPinata();

        // TODO: Play hit FX, sound, shake, squash/stretch here
        
        PlayHitEffect();

        if (currentHealth <= 0)
        {
            BreakPinata();
        }
    }

    private void PlayHitEffect()
    {
        transform.DOComplete();
        transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 1);
    }

    private void BreakPinata()
    {
        Debug.Log("PINATA BROKEN!");
        // TODO: Play explosion, give rewards, transition to next level
    }
}