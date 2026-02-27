using UnityEngine;
using UnityEngine.UI;

public class PowerUpController : MonoBehaviour
{
    [Header("Durations")]
    public float powerUpDuration = 3f;
    public float freezeTimeDuration = 2f;

    private float powerUpTimer;
    private float freezeTimer;

    private bool powerUpActive = false;
    private bool freezeTimeActive = false;

    [Header("UI")]
    public Button invisionButton;
    public Button freezeButton;

    public GameObject invisionLockIcon;
    public GameObject freezeLockIcon;

    [Header("Dependencies")]
    public SnapshotManager snapshot;
    public LevelGenerator generator;
    public PlayerController player;
    public ObstacleMovementController movementController;
    public UIFlowController uiFlowController;

    void Update()
    {
        if (!GameManagerCycle.Instance.gameStateController.IsGameplayActive())
            return;

        if (powerUpActive)
            UpdatePowerUpTimer();

        if (freezeTimeActive)
            UpdateFreezeTimer();
    }

    int GetCurrentWorld()
    {
        return PlayerPrefs.GetInt("SelectedWorld", 1);
    }

    public bool IsInvisionUnlocked()
    {
        return GetCurrentWorld() >= 2;
    }

    public bool IsFreezeUnlocked()
    {
        return GetCurrentWorld() >= 3;
    }

    public bool IsBoosterUnlocked()
    {
        return GetCurrentWorld() >= 3;
    }
    public void UpdatePowerUpUI()
    {
        bool snapshotActive = GameManagerCycle.Instance.IsSnapshotActive;

        // If snapshot is active → disable all powerups
        if (snapshotActive)
        {
            invisionButton.interactable = false;
            freezeButton.interactable = false;
            return;
        }

        // Otherwise apply normal unlock rules
        bool invisionUnlocked = IsInvisionUnlocked();
        invisionButton.interactable = invisionUnlocked;
        invisionLockIcon.SetActive(!invisionUnlocked);

        bool freezeUnlocked = IsFreezeUnlocked();
        freezeButton.interactable = freezeUnlocked;
        freezeLockIcon.SetActive(!freezeUnlocked);
    }

    public void ActivatePowerUp()
    {
        if (!IsInvisionUnlocked())
            return;

        if (powerUpActive)
            return;

        if (freezeTimeActive)
            EndFreezeTime();

        if (GameManagerCycle.Instance.IsSnapshotActive)
        {
            snapshot.ClearSnapshot();
            GameManagerCycle.Instance.SetSnapshotInactive();
        }

        powerUpActive = true;
        powerUpTimer = powerUpDuration;

        snapshot.TakeSnapshot();
        Time.timeScale = 0f;

        uiFlowController.ShowPowerUpMode();
        CameraManager.Instance.EnableTopCamera();
        generator.EnableDragMode(true);

        movementController.OnPowerUpStart();
        GameManagerCycle.Instance.UpdatePlayerMovement();
    }

    void EndPowerUp()
    {
        powerUpActive = false;

        snapshot.ClearSnapshot();
        GameManagerCycle.Instance.SetSnapshotInactive();

        Time.timeScale = 1f;

        uiFlowController.ShowGameplay();
        CameraManager.Instance.EnableMainCamera();
        generator.EnableDragMode(false);

        movementController.OnPowerUpEnd();
        GameManagerCycle.Instance.UpdatePlayerMovement();
    }

    void UpdatePowerUpTimer()
    {
        powerUpTimer -= Time.unscaledDeltaTime;

        if (powerUpTimer <= 0f)
            EndPowerUp();
    }

    public void ActivateFreezeTime()
    {
        if (!IsFreezeUnlocked())
            return;

        if (freezeTimeActive)
            return;

        if (GameManagerCycle.Instance.IsSnapshotActive)
        {
            snapshot.ClearSnapshot();
            GameManagerCycle.Instance.SetSnapshotInactive();
        }

        if (powerUpActive)
            EndPowerUp();

        freezeTimeActive = true;
        freezeTimer = freezeTimeDuration;

        snapshot.TakeSnapshot();

        Time.timeScale = 0f;

        player.canMove = true;
        player.freezeMode = true;
        player.EnableUnscaledAnimation(true);

        movementController.OnFreezeStart();
    }

    void EndFreezeTime()
    {
        freezeTimeActive = false;

        snapshot.ClearSnapshot();
        GameManagerCycle.Instance.SetSnapshotInactive();

        if (!powerUpActive)
            Time.timeScale = 1f;

        player.freezeMode = false;
        player.EnableUnscaledAnimation(false);

        movementController.OnFreezeEnd();
    }

    void UpdateFreezeTimer()
    {
        freezeTimer -= Time.unscaledDeltaTime;

        if (freezeTimer <= 0f)
            EndFreezeTime();
    }

    public bool IsAnyPowerUpActive()
    {
        return powerUpActive || freezeTimeActive;
    }
}