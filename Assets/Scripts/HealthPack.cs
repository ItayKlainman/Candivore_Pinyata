using UnityEngine;
using System;

public class HealthPack : MonoBehaviour
{
    public float healAmount = 20f;
    public float lifeTime = 3.5f;

    public static event Action<float> OnHealthPackTouched;

    void Start()
    {
        Destroy(gameObject, lifeTime); 
    }

    private void OnEnable()
    {
        
    }

    public void TriggerHeal()
    {
        OnHealthPackTouched?.Invoke(healAmount);
        Destroy(gameObject);
    }
}