using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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

    [Header("Animation")]
    public float pulseScale = 1.0f;
    public float pulseDuration = 0.2f;

    private Coroutine invisionPulseRoutine;
    private Coroutine freezePulseRoutine;
    private Coroutine snapshotPulseRoutine;

    [Header("Camera")]
    public CameraFollow cameraFollow;
    void Update()
    {
        if (!GameManagerCycle.Instance.gameStateController.IsGameplayActive())
            return;

        if (powerUpActive)
            UpdatePowerUpTimer();

        if (freezeTimeActive)
            UpdateFreezeTimer();
    }
    bool IsTutorial()
    {
        return TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive;
    }
    int GetCurrentWorld()
    {
        return PlayerPrefs.GetInt("SelectedWorld", 1);
    }

    public bool IsInvisionUnlocked()
    {
        if (IsTutorialLevel())
            return true;
        return GetCurrentWorld() >= 2;
    }

    public bool IsFreezeUnlocked()
    {
        if (IsTutorialLevel())
            return true;
        return GetCurrentWorld() >= 3;
    }

    public bool IsBoosterUnlocked()
    {
        if (IsTutorialLevel())
            return true;
        return GetCurrentWorld() >= 3;
    }

    bool IsTutorialLevel()
    {
        return GameManagerCycle.Instance.levelIndex == 1 && PlayerPrefs.GetInt("TutorialDone", 0) == 0;
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

        if (!IsTutorial())
        {
            if (PowerupInventoryManager.Instance.GetInvisionCount() <= 0)
            {
                uiFlowController.ShowPurchasePanel(PurchaseType.Invision, PurchaseSource.Gameplay);
                return;
            }

            PowerupInventoryManager.Instance.ConsumeInvision();
            AnalyticsManager.LogPowerUpUsed("invision");
        }

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
        AdManager.Instance.ShowBanner();
        uiFlowController.ShowGameplay();
        CameraManager.Instance.EnableMainCamera();
        generator.EnableDragMode(false);

        movementController.OnPowerUpEnd();
        GameManagerCycle.Instance.UpdatePlayerMovement();
        UpdatePowerUpUI();
    }

    void UpdatePowerUpTimer()
    {
        if (IsTutorialLevel())
            return;
        powerUpTimer -= Time.unscaledDeltaTime;

        if (powerUpTimer <= 0f)
            EndPowerUp();
    }

    public void ForceEndPowerUp()
    {
        if (powerUpActive)
        {
            EndPowerUp();
        }
    }

    public void ActivateFreezeTime()
    {
        if (!IsFreezeUnlocked())
            return;

        if (powerUpActive)
            EndPowerUp();

        if (freezeTimeActive)
            return;

        if (!IsTutorial())
        {
            if (PowerupInventoryManager.Instance.GetFreezeCount() <= 0)
            {
                uiFlowController.ShowPurchasePanel(PurchaseType.Freeze, PurchaseSource.Gameplay);
                return;
            }

            PowerupInventoryManager.Instance.ConsumeFreeze();
            AnalyticsManager.LogPowerUpUsed("freeze");
        }

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
        cameraFollow.SetSnapshotView();
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
        cameraFollow.SetGameplayView();
        UpdatePowerUpUI();
    }

    void UpdateFreezeTimer()
    {
        if (IsTutorialLevel())
            return;
        freezeTimer -= Time.unscaledDeltaTime;

        if (freezeTimer <= 0f)
            EndFreezeTime();
    }

    public void EndFreezeFromTutorial()
    {
        if (freezeTimeActive)
        {
            EndFreezeTime();
        }
    }
    public bool IsAnyPowerUpActive()
    {
        return powerUpActive || freezeTimeActive;
    }

    void UpdateCountUI()
    {
        if (IsTutorial())
        {
            invisionCountText.gameObject.SetActive(false);
            invisionPlusIcon.SetActive(false);

            freezeCountText.gameObject.SetActive(false);
            freezePlusIcon.SetActive(false);

            snapshotCountText.gameObject.SetActive(false);
            snapshotAdIcon.SetActive(false);

            return;
        }
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

                if (invisionPulseRoutine != null)
                {
                    StopCoroutine(invisionPulseRoutine);
                    invisionPulseRoutine = null;
                    invisionButton.transform.localScale = Vector3.one;
                }
            }
            else
            {
                invisionCountText.gameObject.SetActive(false);
                invisionPlusIcon.SetActive(true);

                if (invisionPulseRoutine == null)
                    invisionPulseRoutine = StartCoroutine(PulseButton(invisionButton.transform));
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

                if (freezePulseRoutine != null)
                {
                    StopCoroutine(freezePulseRoutine);
                    freezePulseRoutine = null;
                    freezeButton.transform.localScale = Vector3.one;
                }
            }
            else
            {
                freezeCountText.gameObject.SetActive(false);
                freezePlusIcon.SetActive(true);

                if (freezePulseRoutine == null)
                    freezePulseRoutine = StartCoroutine(PulseButton(freezeButton.transform));
            }
        }

        // -------- SNAPSHOT --------
        int snapshotCount = GameManagerCycle.Instance.GetSnapshotUses();
        if (snapshotCount > 0)
        {
            snapshotCountText.text = snapshotCount.ToString();
            snapshotCountText.gameObject.SetActive(true);
            snapshotAdIcon.SetActive(false);

            if (snapshotPulseRoutine != null)
            {
                StopCoroutine(snapshotPulseRoutine);
                snapshotPulseRoutine = null;
                snapshotButton.transform.localScale = Vector3.one;
            }
        }
        else
        {
            snapshotCountText.gameObject.SetActive(false);
            snapshotAdIcon.SetActive(true);

            if (snapshotPulseRoutine == null)
                snapshotPulseRoutine = StartCoroutine(PulseButton(snapshotButton.transform));
        }
    }
    public void OnSnapshotButtonPressed()
    {
        if (GameManagerCycle.Instance.IsSnapshotActive)
            return;

        int snapshotCount = GameManagerCycle.Instance.GetSnapshotUses();

        if (snapshotCount <= 0)
        {
            if (IsTutorial())
            {
                GameManagerCycle.Instance.UseManualSnapshot();
                UpdatePowerUpUI();
                return;
            }

            AdManager.Instance.ShowRewarded(() =>
            {
                GameManagerCycle.Instance.AddSnapshotUse();
                AnalyticsManager.LogEvent("powerup_rewarded",
       ("type", "snapshot"));
                UpdatePowerUpUI();
            });

            return;
        }

        GameManagerCycle.Instance.UseManualSnapshot();
        UpdatePowerUpUI();
    }

    IEnumerator PulseButton(Transform target)
    {
        while (true)
        {
            yield return new WaitForSeconds(4f);

            Vector3 originalScale = target.localScale;
            Vector3 targetScale = originalScale * pulseScale;

            float t = 0f;

            // Scale Up
            while (t < pulseDuration)
            {
                t += Time.unscaledDeltaTime;
                target.localScale = Vector3.Lerp(originalScale, targetScale, t / pulseDuration);
                yield return null;
            }

            t = 0f;

            // Scale Down
            while (t < pulseDuration)
            {
                t += Time.unscaledDeltaTime;
                target.localScale = Vector3.Lerp(targetScale, originalScale, t / pulseDuration);
                yield return null;
            }

            target.localScale = originalScale;
        }
    }
}