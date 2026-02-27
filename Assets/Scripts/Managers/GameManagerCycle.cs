
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerCycle : MonoBehaviour
{
    public static GameManagerCycle Instance;

    [Header("UI Flow")]
    public UIFlowController uiFlowController;

    [Header("Daily Reward")]
    public DailyRewardController dailyRewardController;

    [Header("Progression")]
    public ProgressionController progressionController;

    [Header("State")]
    public GameStateController gameStateController;

    [Header("Obstacle Movement")]
    public ObstacleMovementController movementController;

    [Header("Time Controller")]
    public LevelTimeController levelTimeController;

    [Header("Gameplay References")]
    public SnapshotManager snapshot;
    public LevelGenerator generator;
    public PlayerController player;

    [Header("UI - Menu")]
    public TextMeshProUGUI bestLevelMenuText;

    [Header("UI - Gameplay")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI mapTimerText;

    [Header("UI - Game Over")]
    public TextMeshProUGUI lastLevelText;
    public TextMeshProUGUI bestLevelGameOverText;

    public TextMeshProUGUI coinsEarnedText;

    [Header("Level Unlock System")]
    public int totalLevels = 50;

    [Header("Power Ups")]
    public float powerUpDuration = 3f;
    public float freezeTimeDuration = 2f;
    private float freezeTimer;
    private float powerUpTimer;
    private bool powerUpActive = false;
    private bool freezeTimeActive = false;

    private int levelIndex = 0;
    private int layoutIndex = 0;

    private float mapTimer;
    private float snapshotTimer;

    private bool snapshotActive;

    private WorldData pendingUnlockedWorld;

    [Header("PowerUp Buttons")]
    public Button invisionButton;
    public Button freezeButton;

    [Header("PowerUp Lock Icons")]
    public GameObject invisionLockIcon;
    public GameObject freezeLockIcon;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (!PlayerPrefs.HasKey("GameInitialized"))
        {
            PlayerPrefs.SetInt("UnlockedLevel", 1);
            PlayerPrefs.SetInt("TotalStar", 0);
            PlayerPrefs.SetInt("SelectedWorld", 0);
            PlayerPrefs.SetInt("WorldUnlocked_1", 1);
            PlayerPrefs.SetInt("GameInitialized", 1);
            PlayerPrefs.Save();
        }
        if (PlayerPrefs.GetInt("WorldUnlocked_1", 0) == 0)
        {
            PlayerPrefs.SetInt("WorldUnlocked_1", 1);
            PlayerPrefs.Save();
        }
        Time.timeScale = 1f;
        progressionController.LoadHighestLevel();

        uiFlowController.ShowMenu();
        gameStateController.SetState(GameStateController.GameState.Menu);

        dailyRewardController.Initialize();

        if (dailyRewardController.ShouldAutoShowDailyReward())
        {
            uiFlowController.DisableAllPanels();
            dailyRewardController.dailyRewardPanel.SetActive(true);
            uiFlowController.UpdateHUD(HUDVisibilityController.UIState.Menu);
            return;
        }
    }

    void Update()
    {
        if (!gameStateController.IsGameplayActive()) return;

        if (powerUpActive)
            UpdatePowerUpTimer();
        if (freezeTimeActive)
            UpdateFreezeTimer();
        if (snapshotActive && !freezeTimeActive && !powerUpActive)
            UpdateSnapshotTimer();

        //UpdateLevelTimer();
        UpdateMapTimer();
    }

    void UpdatePlayerMovement()
    {
        player.canMove = gameStateController.IsGameplayActive() && !snapshotActive;
    }

    public void OnDailyRewardButtonClicked()
    {
        dailyRewardController.OnDailyRewardButtonClicked(uiFlowController);
    }

    public void OnStartGameClicked()
    {
        Time.timeScale = 1f;
        StopAllCoroutines();

        snapshot.ClearSnapshot();
        player.ResetPosition();
        uiFlowController.ShowWorldSelect();
        gameStateController.SetState(GameStateController.GameState.WorldSelect);
        player.canMove = false;
    }

    public void OnLevelSelected(int levelNumber)
    {
        levelIndex = levelNumber;
        layoutIndex = 0;

        bool alreadyCompleted =
      PlayerPrefs.GetInt("LevelStars" + levelIndex, 0) > 0;

        if (alreadyCompleted)
        {
            if (!BatteryManager.Instance.HasBattery())
            {
                uiFlowController.ShowNoBatteryPanel();
                return;
            }

            BatteryManager.Instance.ConsumeBattery();
        }

        Time.timeScale = 1f;
        StopAllCoroutines();

        snapshot.ClearSnapshot();
        player.ResetPosition();
        uiFlowController.ShowGameplay();
        LoadLevel();
    }

    void LoadLevel()
    {
        GameEconomyManager.Instance.ResetLevelCoins();

        JsonLevel level = JsonLevelLoader.Instance.GetLevel(levelIndex);

        levelTimeController.StartTimer(level.levelTime);
        mapTimer = level.mapChangeTime;
        snapshotTimer = level.snapshotTime;

        layoutIndex = 0;

        levelText.text = "LEVEL " + (levelIndex);

        mapTimerText.text = mapTimer.ToString("0");

        generator.GenerateFromJson(levelIndex, layoutIndex);

        movementController.InitializeLayout(levelIndex);

        snapshot.ClearSnapshot();
        player.ResetPosition();

        snapshotActive = false;
        gameStateController.SetState(GameStateController.GameState.Gameplay);
        UpdatePlayerMovement();
        StartCoroutine(StartSnapshotNextFrame());
        UpdatePowerUpUI();
    }

    void UpdatePowerUpUI()
    {
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
        {
            Debug.Log("Invision locked until Level 11");
            return;
        }

        if (powerUpActive) return;

        if (freezeTimeActive)
            EndFreezeTime();

        if (snapshotActive)
        {
            snapshot.ClearSnapshot();
            snapshotActive = false;
        }

        powerUpActive = true;
        powerUpTimer = powerUpDuration;

        snapshot.TakeSnapshot();
        Time.timeScale = 0f;

        uiFlowController.ShowPowerUpMode();
        CameraManager.Instance.EnableTopCamera();
        generator.EnableDragMode(true);

        movementController.OnPowerUpStart();
        UpdatePlayerMovement();
    }

    void EndPowerUp()
    {
        powerUpActive = false;

        snapshot.ClearSnapshot();
        snapshotActive = false;
        Time.timeScale = 1f;
        uiFlowController.ShowGameplay();
        CameraManager.Instance.EnableMainCamera();
        generator.EnableDragMode(false);

        movementController.OnPowerUpEnd();

        UpdatePlayerMovement();
    }

    void UpdatePowerUpTimer()
    {
        if (!powerUpActive) return;

        powerUpTimer -= Time.unscaledDeltaTime;

        if (powerUpTimer <= 0f)
        {
            EndPowerUp();
        }
    }

    public void ActivateFreezeTime()
    {
        if (!IsFreezeUnlocked())
        {
            Debug.Log("FreezeTime locked until Level 21");
            return;
        }

        if (freezeTimeActive) return;

        if (snapshotActive)
        {
            snapshot.ClearSnapshot();
            snapshotActive = false;
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
        snapshotActive = false;
        if (!powerUpActive)
            Time.timeScale = 1f;
        player.freezeMode = false;
        player.EnableUnscaledAnimation(false);
        movementController.OnFreezeEnd();
    }

    void UpdateFreezeTimer()
    {
        if (!freezeTimeActive) return;

        freezeTimer -= Time.unscaledDeltaTime;

        if (freezeTimer <= 0f)
        {
            EndFreezeTime();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        gameStateController.SetState(GameStateController.GameState.Pause);
        uiFlowController.PauseGame();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        gameStateController.SetState(GameStateController.GameState.Gameplay);
        uiFlowController.ResumeGame();
    }

    public void PlayerReachedDoor()
    {
        if (gameStateController.CurrentState != GameStateController.GameState.Gameplay)
            return;

        OnLevelCompleted();
    }

    void OnLevelCompleted()
    {
        levelTimeController.StopTimer();
        progressionController.ClearLevelFailed(levelIndex);

        gameStateController.SetState(GameStateController.GameState.LevelComplete);
        player.canMove = false;
        player.StopMovementImmediately();
        progressionController.CalculateStars(levelTimeController.GetRemainingTime());
        progressionController.GiveCoinsForStars();

        progressionController.UnlockNextLevel(levelIndex, totalLevels);
        uiFlowController.ShowLevelComplete();

        progressionController.CheckForNewWorldUnlock();
        int earnedCoins = GameEconomyManager.Instance.GetLevelCoins();
        coinsEarnedText.text = "COINS EARNED :    " + earnedCoins;

        Debug.Log("Coins Earned: " + earnedCoins);
    }

    public void OnNextLevelButton()
    {
        int levelsPerWorld = 10;

        bool isLastLevelOfWorld = (levelIndex % levelsPerWorld == 0);

        if (isLastLevelOfWorld)
        {
            int currentWorld = PlayerPrefs.GetInt("SelectedWorld", 1);
            int nextWorld = currentWorld + 1;

            if (nextWorld <= WorldDatabase.Instance.GetWorlds().Count)
            {
                PlayerPrefs.SetInt("SelectedWorld", nextWorld);
                PlayerPrefs.Save();

                uiFlowController.ShowLevelSelect();
                return;
            }
        }
        levelIndex++;

        JsonLevel level = JsonLevelLoader.Instance.GetLevel(levelIndex);
        if (level == null)
        {
            uiFlowController.ShowMenu();
            return;
        }

        layoutIndex = 0;
        progressionController.UpdateHighestLevel(levelIndex);

        uiFlowController.ShowGameplay();
        LoadLevel();
    }

    public void PlayerHitObstacle()
    {
        if (!gameStateController.IsGameplayActive()) return;

        gameStateController.SetState(GameStateController.GameState.GameOver);
        player.canMove = false;

        player.PlayHitAnimation();

        StartCoroutine(GameOverAfterDeathSequence());
    }

    IEnumerator GameOverAfterDeathSequence()
    {
        yield return new WaitForSeconds(1.6f);

        movementController.OnGameOver();

        yield return new WaitForSeconds(0.3f);
        levelTimeController.StopTimer();
        uiFlowController.ShowGameOver();
    }

    public void Retry()
    {
        levelTimeController.StopTimer();

        if (!BatteryManager.Instance.HasBattery())
        {
            uiFlowController.ShowNoBatteryPanel();
            return;
        }

        BatteryManager.Instance.ConsumeBattery();

        Time.timeScale = 1f;
        StopAllCoroutines();

        snapshot.ClearSnapshot();
        player.ResetPosition();
        layoutIndex = 0;

        uiFlowController.ShowGameplay();
        LoadLevel();

        movementController.ResetState();
    }

    public void StartSnapshot()
    {
        if (snapshotActive || powerUpActive || freezeTimeActive) return;

        JsonLevel level = JsonLevelLoader.Instance.GetLevel(levelIndex);
        snapshotTimer = level.snapshotTime;

        snapshot.TakeSnapshot();
        snapshotActive = true;

        movementController.OnSnapshotStart();

        UpdatePlayerMovement();

        Debug.Log("Snapshot Time = " + snapshotTimer);
    }

    void UpdateSnapshotTimer()
    {
        snapshotTimer -= Time.deltaTime;

        if (snapshotTimer <= 0)
        {
            snapshot.ClearSnapshot();
            snapshotActive = false;

            movementController.OnSnapshotEnd();

            UpdatePlayerMovement();
        }
    }

    IEnumerator StartSnapshotNextFrame()
    {
        yield return null;

        if (!gameStateController.IsGameplayActive()) yield break;

        StartSnapshot();
    }

    void UpdateMapTimer()
    {
        mapTimer -= Time.deltaTime;
        mapTimerText.text = mapTimer.ToString("0");

        if (mapTimer <= 0)
        {
            layoutIndex++;

            JsonLevel level = JsonLevelLoader.Instance.GetLevel(levelIndex);

            if (layoutIndex >= level.layouts.Count)
                layoutIndex = 0;

            generator.GenerateFromJson(levelIndex, layoutIndex);

            movementController.InitializeLayout(levelIndex);

            snapshot.ClearSnapshot();
            snapshotActive = false;

            StartCoroutine(StartSnapshotNextFrame());
            mapTimer = level.mapChangeTime;
        }
    }

    public void BoosterCollected()
    {
        generator.DestroyAllObstacles();
        generator.DestroyBooster();
    }

    public void RevivePlayer()
    {
        levelTimeController.StopTimer();
        StopAllCoroutines();

        Time.timeScale = 1f;

        snapshot.ClearSnapshot();
        freezeTimeActive = false;
        powerUpActive = false;
        snapshotActive = false;

        player.ReviveToLastSafeTile();
        player.StopMovementImmediately();

        uiFlowController.ShowGameplay();
        levelText.text = "LEVEL " + levelIndex;

        gameStateController.SetState(GameStateController.GameState.Gameplay);
        player.canMove = true;
        movementController.ResetState();
        movementController.InitializeLayout(levelIndex);
    }

    public void OnNewWorldYesClicked()
    {
        if (pendingUnlockedWorld == null)
            return;

        int worldId = pendingUnlockedWorld.worldId;

        PlayerPrefs.SetInt("SelectedWorld", worldId);
        PlayerPrefs.Save();

        pendingUnlockedWorld = null;

        uiFlowController.ShowLevelSelect();
        gameStateController.SetState(GameStateController.GameState.LevelSelect);
    }

    public void OnNewWorldNoClicked()
    {
        pendingUnlockedWorld = null;
        OnNextLevelButton();
    }

    public int CurrentLevelNumber
    {
        get { return levelIndex; }
    }

    #region PowerUp Unlock System

    public bool IsInvisionUnlocked()
    {
        return levelIndex >= 11;
    }

    public bool IsFreezeUnlocked()
    {
        return levelIndex >= 21;
    }

    public bool IsBoosterUnlocked()
    {
        return levelIndex >= 21;
    }

    #endregion
    public void OnWorldSelected(int worldId)
    {
        PlayerPrefs.SetInt("SelectedWorld", worldId);
        PlayerPrefs.Save();

        uiFlowController.OpenLevelPanel();
    }
    public void ShowNewWorldUnlockedPanel(WorldData world)
    {
        pendingUnlockedWorld = world;
        uiFlowController.ShowNewWorldPanel(world);
    }
    public void HandleTimeOut()
    {
        gameStateController.SetState(GameStateController.GameState.GameOver);

        player.canMove = false;
        player.StopMovementImmediately();

        movementController.OnGameOver();
        levelTimeController.StopTimer();

        uiFlowController.ShowGameOver();
    }
}
