using UnityEngine;

public static class HapticsManager
{
    public static bool HapticsEnabled = true;

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject vibrator;

    static HapticsManager()
    {
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        }
    }
#endif

    public static void LightImpact()
    {
        if (!HapticsEnabled) return;
        Vibrate(30);
    }

    public static void MediumImpact()
    {
        if (!HapticsEnabled) return;
        Vibrate(60);
    }

    public static void HeavyImpact()
    {
        if (!HapticsEnabled) return;
        Vibrate(100);
    }

    private static void Vibrate(long milliseconds)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (vibrator != null)
            vibrator.Call("vibrate", milliseconds);
#elif UNITY_IOS
        // iOS requires plugin or native bridge, placeholder:
        Handheld.Vibrate();
#else
        Handheld.Vibrate(); // fallback in editor/unsupported platforms
#endif
    }
}