using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance;

    [SerializeField] private CinemachineVirtualCamera virtualCam;
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeAmplitude = 1.2f;
    [SerializeField] private float shakeFrequency = 2.0f;

    private CinemachineBasicMultiChannelPerlin noise;
    private float shakeTimer;
    private bool isShaking = false;

    public static bool ShakeEnabled
    {
        get => PlayerPrefs.GetInt("ShakeEnabled", 1) == 1;
        set
        {
            PlayerPrefs.SetInt("ShakeEnabled", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        noise = virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        StopShake();
    }

    private void Update()
    {
        if (!isShaking) return;

        shakeTimer -= Time.unscaledDeltaTime;
        if (shakeTimer <= 0f)
        {
            StopShake();
        }
    }

    public void Shake(float intensity = -1f, float duration = -1f)
    {
        if (!ShakeEnabled || noise == null) return;

        noise.m_AmplitudeGain = intensity > 0 ? intensity : shakeAmplitude;
        noise.m_FrequencyGain = shakeFrequency;
        shakeTimer = duration > 0 ? duration : shakeDuration;
        isShaking = true;
    }

    private void StopShake()
    {
        if (noise != null)
        {
            noise.m_AmplitudeGain = 0f;
            noise.m_FrequencyGain = 0f;
        }

        isShaking = false;
    }
}