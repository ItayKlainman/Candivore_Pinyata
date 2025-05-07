using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class OptionsUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private RectTransform panelContent;
    [SerializeField] private CanvasGroup panelCanvasGroup;

    [Header("Buttons")]
    [SerializeField] private Button openMainMenuButton;
    [SerializeField] private Button openInGameButton;
    [SerializeField] private Button closeButton;

    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Toggles")]
    [SerializeField] private Toggle hapticsToggle;
    
    [Header("Icon Reactions")]
    [SerializeField] private RectTransform musicIcon;
    [SerializeField] private RectTransform sfxIcon;
    [SerializeField] private RectTransform hapticsIcon;
    
    [SerializeField] private Button returnToMainMenuButton;
    
    private const string HAPTICS_KEY = "HapticsEnabled";
    private bool wasPausedBefore = false;
    private bool openedFromGame = false;
    
    private void Start()
    {
        // Load volume values
        musicSlider.value = AudioManager.Instance.musicVolume;
        sfxSlider.value = AudioManager.Instance.sfxVolume;

        // Load haptics
        bool hapticsEnabled = PlayerPrefs.GetInt(HAPTICS_KEY, 1) == 1;
        hapticsToggle.isOn = hapticsEnabled;
        HapticsManager.HapticsEnabled = hapticsEnabled;

        // Listeners
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        hapticsToggle.onValueChanged.AddListener(OnHapticsToggled);

        openMainMenuButton.onClick.AddListener(() =>
        {
            openedFromGame = false;
            OpenOptions();
        });

        openInGameButton.onClick.AddListener(() =>
        {
            openedFromGame = true;
            OpenOptions();
        });

        closeButton.onClick.AddListener(CloseOptions);
        
        returnToMainMenuButton.onClick.AddListener(HandleReturnToMainMenu);
        
        optionsPanel.SetActive(false);
        panelCanvasGroup.alpha = 0;
        panelContent.localScale = Vector3.zero;
    }
    
    private void HandleReturnToMainMenu()
    {
        Time.timeScale = 1f;
        GameStateManager.Instance.SetGameState(GameState.Menu); 
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene"); 
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.musicVolume = value;
        AudioManager.Instance.UpdateMusicVolume();
        AudioManager.Instance.SaveVolumeSettings();
    }

    private void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.sfxVolume = value;
        AudioManager.Instance.SaveVolumeSettings();
    }

    private void OnHapticsToggled(bool enabled)
    {
        HapticsManager.HapticsEnabled = enabled;
        PlayerPrefs.SetInt(HAPTICS_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();

        FeedbackManager.Play("Toggle", FeedbackStrength.Light); // NEW
        PunchIcon(hapticsIcon);
    }


    private void OpenOptions()
    {
        wasPausedBefore = Time.timeScale == 0;
        optionsPanel.SetActive(true);
        returnToMainMenuButton.gameObject.SetActive(openedFromGame);
        
        FeedbackManager.Play("Popup", FeedbackStrength.Light);

        panelCanvasGroup.alpha = 0;
        panelContent.localScale = Vector3.zero;

        panelCanvasGroup.DOFade(1f, 0.25f).SetUpdate(true);
        panelContent.DOScale(1f, 0.4f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                Time.timeScale = 0;
            });
    }

    
    private void CloseOptions()
    {
        FeedbackManager.Play("Popup", FeedbackStrength.Light); 

        panelCanvasGroup.DOFade(0f, 0.2f).SetUpdate(true);
        panelContent.DOScale(0f, 0.3f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                optionsPanel.SetActive(false);
                if (!wasPausedBefore)
                    Time.timeScale = 1;
            });
    }

    
    public void OnMusicSliderReleased(BaseEventData data)
    {
        PunchIcon(musicIcon);
        FeedbackManager.Play("Slider", FeedbackStrength.Light);
    }


    public void OnSFXSliderReleased(BaseEventData data)
    {
        PunchIcon(sfxIcon);
        FeedbackManager.Play("Slider", FeedbackStrength.Light); 
    }

    
    private void PunchIcon(RectTransform icon)
    {
        if (icon == null) return;

        icon.DOKill();
        icon.DOPunchScale(Vector3.one * 0.15f, 0.3f, 8, 0.8f).SetUpdate(true);
    }

}
