using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    
    [SerializeField] private GameObject upgradePanel;

    void Start()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        optionsButton.onClick.AddListener(OnOptionsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    void OnPlayClicked()
    {
        mainMenuPanel.SetActive(false);
        upgradePanel.SetActive(true);
        GameStateManager.Instance.SetGameState(GameState.Start);
    }

    void OnOptionsClicked()
    {
        Debug.Log("Options clicked — not implemented yet.");
    }

    void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}