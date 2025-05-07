using UnityEngine;
using System;

public class HealthPack : MonoBehaviour
{
    public float healAmount = 20f;
    public float lifeTime = 3.5f;

    public static event Action<float> OnHealthPackTouched;

    [SerializeField] private Color healTextColor = Color.green; 

    public void TriggerHeal()
    {
        OnHealthPackTouched?.Invoke(healAmount);

        // Spawn floating heal text
        Vector3 spawnPos = transform.position + Vector3.up * 1.2f;
        var floatText = ObjectPool.Instance.GetFromPool("Text", spawnPos, Quaternion.identity);
        floatText.GetComponent<FloatingText>()?.Show($"+{healAmount}", healTextColor, spawnPos);
        FeedbackManager.Play("Health", FeedbackStrength.Light, 0.7f);

        ObjectPool.Instance.ReturnToPool("HealthPack", gameObject);
    }
}