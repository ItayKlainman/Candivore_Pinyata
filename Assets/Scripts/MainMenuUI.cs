using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    
    [SerializeField] private GameObject upgradePanel;

    private void Start()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
        
        AudioManager.Instance.PlaySFX("MainTheme", 0.8f, true);
    }

    private void OnPlayClicked()
    {
        AnimateButton(playButton);
        FeedbackManager.Play("ButtonClick", FeedbackStrength.Light, 0.7f);
        mainMenuPanel.SetActive(false);
        upgradePanel.SetActive(true);
        GameStateManager.Instance.SetGameState(GameState.Start);
    }

    private void OnQuitClicked()
    {
        AnimateButton(quitButton);
        FeedbackManager.Play("ButtonClick", FeedbackStrength.Light, 0.7f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
    
    private void AnimateButton(Button button)
    {
        var rect = button.GetComponent<RectTransform>();
        rect.DOKill(); 
        rect.DOScale(1.1f, 0.1f).SetLoops(2, LoopType.Yoyo);
    }
    
}