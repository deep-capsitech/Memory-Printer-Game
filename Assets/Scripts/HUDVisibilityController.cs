using UnityEngine;

public class HUDVisibilityController : MonoBehaviour
{
    [Header("HUD Elements")]
    public GameObject batteryBar;
    public GameObject coinPanel;

    public enum UIState
    {
        Menu,
        World,
        Level,
        Gameplay,
        Pause,
        Revive,
        GameOver,
        LevelComplete
    }

    public void UpdateHUD(UIState state)
    {
        // Battery visibility
        bool showBattery =
            state == UIState.GameOver ||
            state == UIState.Level ||
            state == UIState.Revive ||
            state == UIState.World ||
            state == UIState.LevelComplete;

        batteryBar.SetActive(showBattery);

        // Coin visibility
        bool showCoins =
            state == UIState.Menu ||
            state == UIState.Level ||
            state == UIState.Revive;

        coinPanel.SetActive(showCoins);
    }
}
