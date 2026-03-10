using UnityEngine;

public class GameStateController : MonoBehaviour
{
    public enum GameState
    {
        Menu,
        WorldSelect,
        LevelSelect,
        Gameplay,
        Pause,
        GameOver,
        LevelComplete
    }

    public GameState CurrentState { get; private set; }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;
        Debug.Log("Game State Changed → " + newState);
    }

    public bool IsGameplayActive()
    {
        return CurrentState == GameState.Gameplay;
    }

    public bool IsPaused()
    {
        return CurrentState == GameState.Pause;
    }

    public bool IsMenu()
    {
        return CurrentState == GameState.Menu;
    }
}