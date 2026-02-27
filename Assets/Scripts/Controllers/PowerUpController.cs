using UnityEngine;

public class PowerUpController : MonoBehaviour
{
    [Header("Dependencies")]
    public SnapshotManager snapshot;
    public UIFlowController uiFlowController;
    public CameraManager cameraManager;
    public LevelGenerator generator;
    public ObstacleMovementController movementController;
    public PlayerController player;
    public GameStateController gameStateController;

    [Header("Settings")]
    public float powerUpDuration = 3f;

    private float powerUpTimer;
    private bool powerUpActive;

    public bool IsActive => powerUpActive;

    // ================================
    // ACTIVATE
    // ================================
    public void Activate()
    {
        if (powerUpActive) return;
        if (!gameStateController.IsGameplayActive()) return;

        powerUpActive = true;
        powerUpTimer = powerUpDuration;

        snapshot.TakeSnapshot();
        Time.timeScale = 0f;

        uiFlowController.DisableAllPanels();
        cameraManager.EnableTopCamera();
        generator.EnableDragMode(true);

        movementController.OnPowerUpStart();
        player.canMove = false;
    }

    // ================================
    // UPDATE
    // ================================
    void Update()
    {
        if (!powerUpActive) return;

        powerUpTimer -= Time.unscaledDeltaTime;

        if (powerUpTimer <= 0f)
        {
            End();
        }
    }

    // ================================
    // END
    // ================================
    public void End()
    {
        if (!powerUpActive) return;

        powerUpActive = false;

        snapshot.ClearSnapshot();
        Time.timeScale = 1f;

        cameraManager.EnableMainCamera();
        generator.EnableDragMode(false);

        movementController.OnPowerUpEnd();
    }
}