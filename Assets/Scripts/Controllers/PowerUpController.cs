using TMPro;
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

    [Header("Count UI")]
    public TextMeshProUGUI invisionCountText;
    public GameObject invisionPlusIcon;

    public TextMeshProUGUI freezeCountText;
    public GameObject freezePlusIcon;

    public Button snapshotButton;

    [Header("Snapshot UI")]
    public TextMeshProUGUI snapshotCountText;
    public GameObject snapshotAdIcon;

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

        if (snapshotActive)
        {
            invisionButton.interactable = false;
            freezeButton.interactable = false;
            return;
        }

        bool invisionUnlocked = IsInvisionUnlocked();
        invisionButton.interactable = invisionUnlocked;
        invisionLockIcon.SetActive(!invisionUnlocked);

        bool freezeUnlocked = IsFreezeUnlocked();
        freezeButton.interactable = freezeUnlocked;
        freezeLockIcon.SetActive(!freezeUnlocked);

        UpdateCountUI();
    }

    public void ActivatePowerUp()
    {
        if (!IsInvisionUnlocked())
            return;

        if (powerUpActive)
            return;

        if (freezeTimeActive)
            EndFreezeTime();

        if (PowerupInventoryManager.Instance.GetInvisionCount() <= 0)
        {
            uiFlowController.ShowBuyPowerupPanel(PowerupType.Invision);
            return;
        }

        PowerupInventoryManager.Instance.ConsumeInvision();

        if (GameManagerCycle.Instance.IsSnapshotActive)
        {
            snapshot.ClearSnapshot();
            GameManagerCycle.Instance.SetSnapshotInactive();
        }

        powerUpActive = true;
        powerUpTimer = powerUpDuration;

        snapshot.TakeSnapshot();
        Time.timeScale = 0f;
        AdManager.Instance.HideBanner();
        uiFlowController.ShowPowerUpMode();
        CameraManager.Instance.EnableTopCamera();
        generator.EnableDragMode(true);

        movementController.OnPowerUpStart();
        GameManagerCycle.Instance.UpdatePlayerMovement();

        UpdatePowerUpUI();
    }

    void EndPowerUp()
    {
        powerUpActive = false;

        snapshot.ClearSnapshot();
        GameManagerCycle.Instance.SetSnapshotInactive();

        Time.timeScale = 1f;
        AdManager .Instance.ShowBanner();
        uiFlowController.ShowGameplay();
        CameraManager.Instance.EnableMainCamera();
        generator.EnableDragMode(false);

        movementController.OnPowerUpEnd();
        GameManagerCycle.Instance.UpdatePlayerMovement();
        UpdatePowerUpUI();
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

        if (powerUpActive)
            EndPowerUp();

        if (freezeTimeActive)
            return;

        if (PowerupInventoryManager.Instance.GetFreezeCount() <= 0)
        {
            uiFlowController.ShowBuyPowerupPanel(PowerupType.Freeze);
            return;
        }

        PowerupInventoryManager.Instance.ConsumeFreeze();

        if (GameManagerCycle.Instance.IsSnapshotActive)
        {
            snapshot.ClearSnapshot();
            GameManagerCycle.Instance.SetSnapshotInactive();
        }

        freezeTimeActive = true;
        freezeTimer = freezeTimeDuration;

        snapshot.TakeSnapshot();

        Time.timeScale = 0f;

        player.canMove = true;
        player.freezeMode = true;
        player.EnableUnscaledAnimation(true);

        movementController.OnFreezeStart();
        UpdateCountUI();
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
        UpdatePowerUpUI();
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

    void UpdateCountUI()
    {
        bool invisionUnlocked = IsInvisionUnlocked();
        bool freezeUnlocked = IsFreezeUnlocked();

        // -------- INVISION --------
        if (!invisionUnlocked)
        {
            invisionCountText.gameObject.SetActive(false);
            invisionPlusIcon.SetActive(false);
        }
        else
        {
            int invisionCount = PowerupInventoryManager.Instance.GetInvisionCount();

            if (invisionCount > 0)
            {
                invisionCountText.text = invisionCount.ToString();
                invisionCountText.gameObject.SetActive(true);
                invisionPlusIcon.SetActive(false);
            }
            else
            {
                invisionCountText.gameObject.SetActive(false);
                invisionPlusIcon.SetActive(true);
            }
        }

        // -------- FREEZE --------
        if (!freezeUnlocked)
        {
            freezeCountText.gameObject.SetActive(false);
            freezePlusIcon.SetActive(false);
        }
        else
        {
            int freezeCount = PowerupInventoryManager.Instance.GetFreezeCount();

            if (freezeCount > 0)
            {
                freezeCountText.text = freezeCount.ToString();
                freezeCountText.gameObject.SetActive(true);
                freezePlusIcon.SetActive(false);
            }
            else
            {
                freezeCountText.gameObject.SetActive(false);
                freezePlusIcon.SetActive(true);
            }
        }

        // -------- SNAPSHOT --------
        int snapshotCount = GameManagerCycle.Instance.GetSnapshotUses();

        if (snapshotCount > 0)
        {
            snapshotCountText.text = snapshotCount.ToString();
            snapshotCountText.gameObject.SetActive(true);
            snapshotAdIcon.SetActive(false);
        }
        else
        {
            snapshotCountText.gameObject.SetActive(false);
            snapshotAdIcon.SetActive(true);
        }
    }
    public void OnSnapshotButtonPressed()
    {
        if (GameManagerCycle.Instance.IsSnapshotActive)
            return;

        int snapshotCount = GameManagerCycle.Instance.GetSnapshotUses();

        if (snapshotCount <= 0)
        {
            AdManager.Instance.ShowRewarded(() =>
            {
                GameManagerCycle.Instance.AddSnapshotUse();
                UpdatePowerUpUI();
            });

            return;
        }

        GameManagerCycle.Instance.UseManualSnapshot();
        UpdatePowerUpUI();
    }
}