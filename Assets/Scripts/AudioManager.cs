using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class NamedAudioClip
{
    public string name;
    public AudioClip clip;
    public bool loop = false;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Clips")]
    [SerializeField] private List<NamedAudioClip> audioClips = new();

    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;

    private Dictionary<string, NamedAudioClip> clipLookup;
    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY = "SFXVolume";

    private float currentlyPlayingMusicVolume;
    private bool musicStarted = false;
    private string currentClipName = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildLookup();
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);
    }

    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, musicVolume);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, sfxVolume);
        PlayerPrefs.Save();
        UpdateMusicVolume();
    }

    private void BuildLookup()
    {
        clipLookup = new Dictionary<string, NamedAudioClip>();
        foreach (var entry in audioClips)
        {
            if (!clipLookup.ContainsKey(entry.name))
                clipLookup.Add(entry.name, entry);
        }
    }

    public void UpdateMusicVolume()
    {
        if (musicSource != null)
            musicSource.volume = currentlyPlayingMusicVolume * musicVolume;
    }

    public void PlaySFX(string name, float volume = 1f, bool isMusic = false, float pitchVariation = 0f)
    {
        if (clipLookup == null || !clipLookup.ContainsKey(name))
        {
            Debug.LogWarning($"Sound '{name}' not found.");
            return;
        }

        var clipEntry = clipLookup[name];

        if (isMusic)
        {
            if (musicStarted && currentClipName == name)
                return; // music already playing

            currentClipName = name;
            currentlyPlayingMusicVolume = volume;

            musicSource.clip = clipEntry.clip;
            musicSource.loop = clipEntry.loop;
            musicSource.volume = 0f;
            musicSource.Play();
            StartCoroutine(FadeMusicCoroutine(volume * musicVolume, 1f));

            musicStarted = true;
        }
        else
        {
            float pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clipEntry.clip, volume * sfxVolume);
            sfxSource.pitch = 1f;
        }
    }

    public void FadeOutMusic(float duration = 1f)
    {
        if (musicSource == null || !musicSource.isPlaying) return;
        StartCoroutine(FadeMusicCoroutine(0f, duration, stopAfterFade: true));
    }

    public void FadeInMusic(string name, float volume = 1f, float duration = 1f)
    {
        if (!clipLookup.ContainsKey(name)) return;

        var clipEntry = clipLookup[name];
        if (musicSource.clip != clipEntry.clip)
        {
            musicSource.clip = clipEntry.clip;
            musicSource.loop = clipEntry.loop;
            musicSource.Play();
            currentClipName = name;
        }

        musicStarted = true;
        currentlyPlayingMusicVolume = volume;
        StartCoroutine(FadeMusicCoroutine(volume * musicVolume, duration));
    }

    private IEnumerator FadeMusicCoroutine(float targetVolume, float duration, bool stopAfterFade = false)
    {
        float startVolume = musicSource.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;

        if (targetVolume <= 0f && stopAfterFade)
        {
            musicSource.Stop();
            musicStarted = false;
            currentClipName = "";
        }
    }
}
