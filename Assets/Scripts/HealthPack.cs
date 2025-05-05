using UnityEngine;
using System;

public class HealthPack : MonoBehaviour
{
    public float healAmount = 20f;
    public float lifeTime = 3.5f;

    public static event Action<float> OnHealthPackTouched;
    
    public void TriggerHeal()
    {
        OnHealthPackTouched?.Invoke(healAmount);
        ObjectPool.Instance.ReturnToPool("HealthPack", gameObject);
    }
}