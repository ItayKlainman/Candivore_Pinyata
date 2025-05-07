using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance;

    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float shakeDuration = 0.2f;

    private float shakeTimer;
    private CinemachineBasicMultiChannelPerlin perlin;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (virtualCamera != null)
        {
            perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    public void Shake(float amplitude, float frequency)
    {
        if (perlin == null) return;

        perlin.m_AmplitudeGain = amplitude;
        perlin.m_FrequencyGain = frequency;
        shakeTimer = shakeDuration;
    }

    private void Update()
    {
        if (perlin == null) return;

        if (shakeTimer > 0)
        {
            shakeTimer -= Time.unscaledDeltaTime;
            if (shakeTimer <= 0f)
            {
                perlin.m_AmplitudeGain = 0f;
                perlin.m_FrequencyGain = 0f;
            }
        }
    }
}