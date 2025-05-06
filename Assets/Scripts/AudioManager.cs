using UnityEngine;
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
    

    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, musicVolume);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, sfxVolume);
        PlayerPrefs.Save();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildLookup();
            musicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1f);
            sfxVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);
        }
        else
        {
            Destroy(gameObject);
        }
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
            musicSource.volume =  currentlyPlayingMusicVolume * musicVolume;
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
            musicSource.clip = clipEntry.clip;
            musicSource.volume = volume * musicVolume;
            musicSource.loop = clipEntry.loop;
            currentlyPlayingMusicVolume = volume;
            musicSource.Play();
        }
        else
        {
            float pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clipEntry.clip, volume * sfxVolume);
            sfxSource.pitch = 1f; // reset to normal
        }
    }
}