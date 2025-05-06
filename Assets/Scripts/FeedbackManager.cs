using UnityEngine;

public enum FeedbackStrength { Light, Medium, Heavy }

public static class FeedbackManager
{
    public static void Play(string soundName, FeedbackStrength strength, float volume = 1f, float pitchVar = 0.1f,bool isMusic = false)
    {
        AudioManager.Instance.PlaySFX(soundName, volume, isMusic, pitchVar);

        switch (strength)
        {
            case FeedbackStrength.Light:
                HapticsManager.LightImpact();
                break;
            case FeedbackStrength.Medium:
                HapticsManager.MediumImpact();
                break;
            case FeedbackStrength.Heavy:
                HapticsManager.HeavyImpact();
                break;
        }

        // Optional: Add camera shake here later
    }
}