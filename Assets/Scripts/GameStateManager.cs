using UnityEngine;

public enum GameState
{
    Menu,
    Start,
    Playing,
    Ended,
    Upgrading
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;
    private GameState CurrentState { get; set; }

    [SerializeField] private LevelManager levelManager;
    [SerializeField] private UpgradeUIManager upgradeUIManager;
    
    public delegate void GameStateChanged(GameState newState);
    public event GameStateChanged OnGameStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        levelManager.Initialize();
        upgradeUIManager.Initialize();
    }

    public void SetGameState(GameState newState)
    {
        if (newState == CurrentState) return;

        CurrentState = newState;
        Debug.Log("Game State changed to: " + newState);

        OnGameStateChanged?.Invoke(newState);
    }

    public bool IsState(GameState state)
    {
        return CurrentState == state;
    }
}