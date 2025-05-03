using UnityEngine;

public enum GameState
{
    Start,
    Playing,
    Ended,
    Upgrading
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;
    private GameState CurrentState { get; set; }

    public delegate void GameStateChanged(GameState newState);
    public event GameStateChanged OnGameStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetGameState(GameState.Start);
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