
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86;

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

    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject gameplayPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;
    public GameObject worldPanel;

    [Header("Gameplay References")]
    public SnapshotManager snapshot;
    public LevelGenerator generator;
    public PlayerController player;

    [Header("UI - Menu")]
    public TextMeshProUGUI bestLevelMenuText;

    [Header("UI - Gameplay")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI mapTimerText;

    [Header("UI - Game Over")]
    public TextMeshProUGUI lastLevelText;
    public TextMeshProUGUI bestLevelGameOverText;

    [Header("Star UI")]
    public Image star1;
    public Image star2;
    public Image star3;

    public Sprite filledStar;
    public Sprite emptyStar;

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

    // private float levelTimer;
    private float mapTimer;
    private float snapshotTimer;

    private bool snapshotActive;

    [Header("HUD")]
    public HUDVisibilityController hud;

    [Header("UI Background")]
    public GameObject backgroundPanel;

    [Header("New Dynamic Level Panel")]
    public GameObject levelPanel;

    //private HashSet<MovingObstacle> movingObstaclesForLayout = new HashSet<MovingObstacle>();

    [Header("No Battery Panel")]
    public GameObject noBatteryPanel;

    [Header("New World Unlock Panel")]
    public GameObject newWorldPanel;
    public TextMeshProUGUI newWorldNameText;
    public TextMeshProUGUI newWorldQuestionText;

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
            PlayerPrefs.SetInt("GameInitialized", 1);
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

        uiFlowController.DisableAllPanels();
        worldPanel.SetActive(true);
        uiFlowController.UpdateHUD(HUDVisibilityController.UIState.World);
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
                uiFlowController.ShowNoBatteryPanel(gameplayPanel);
                return;
            }

            BatteryManager.Instance.ConsumeBattery();
        }

        Time.timeScale = 1f;
        StopAllCoroutines();

        snapshot.ClearSnapshot();
        player.ResetPosition();

        uiFlowController.DisableAllPanels();
        backgroundPanel.SetActive(false);
        gameplayPanel.SetActive(true);

        uiFlowController.UpdateHUD(HUDVisibilityController.UIState.Gameplay);
        // gameStateController.SetState(GameStateController.GameState.Gameplay);
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
        //  timerText.text = levelTimer.ToString("0");
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
        // -------- INVISION --------
        bool invisionUnlocked = IsInvisionUnlocked();

        invisionButton.interactable = invisionUnlocked;
        invisionLockIcon.SetActive(!invisionUnlocked);


        // -------- FREEZE --------
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
        uiFlowController.DisableAllPanels();
        backgroundPanel.SetActive(false);
        CameraManager.Instance.EnableTopCamera();
        generator.EnableDragMode(true);

        movementController.OnPowerUpStart();
        UpdatePlayerMovement();
    }

    void EndPowerUp()
    {
        powerUpActive = false;
        //snapshotActive = false;

        snapshot.ClearSnapshot();
        //player.canMove = true;
        Time.timeScale = 1f;
        gameplayPanel.SetActive(true);
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
        //player.canMove = true;
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
        //StartCoroutine(LevelCompleteSequence());
        OnLevelCompleted();
    }
    void OnLevelCompleted()
    {
        levelTimeController.StopTimer();
        ClearLevelFailed(levelIndex);

        gameStateController.SetState(GameStateController.GameState.LevelComplete);
        player.canMove = false;
        progressionController.CalculateStars(levelTimeController.GetRemainingTime());
        progressionController.GiveCoinsForStars();

        progressionController.UnlockNextLevel(levelIndex, totalLevels);

        uiFlowController.DisableAllPanels();
        levelCompletePanel.SetActive(true);
        uiFlowController.UpdateHUD(HUDVisibilityController.UIState.LevelComplete);
        progressionController.CheckForNewWorldUnlock();
        int earnedCoins = GameEconomyManager.Instance.GetLevelCoins();
        coinsEarnedText.text = "COINS EARNED :    " + earnedCoins;

        Debug.Log("Coins Earned: " + earnedCoins);
    }
    public void OnNextLevelButton()
    {
        int levelsPerWorld = 10;

        // Check if this was the last level of the current world
        bool isLastLevelOfWorld = (levelIndex % levelsPerWorld == 0);

        if (isLastLevelOfWorld)
        {
            int currentWorld = PlayerPrefs.GetInt("SelectedWorld", 1);
            int nextWorld = currentWorld + 1;

            // If next world exists
            if (nextWorld <= WorldDatabase.Instance.GetWorlds().Count)
            {
                PlayerPrefs.SetInt("SelectedWorld", nextWorld);
                PlayerPrefs.Save();

                uiFlowController.DisableAllPanels();
                levelPanel.SetActive(true);
                uiFlowController.UpdateHUD(HUDVisibilityController.UIState.Level);

                return;
            }
        }

        // Normal next-level flow
        levelIndex++;

        JsonLevel level = JsonLevelLoader.Instance.GetLevel(levelIndex);
        if (level == null)
        {
            uiFlowController.ShowMenu();
            return;
        }

        layoutIndex = 0;
        progressionController.UpdateHighestLevel(levelIndex);

        uiFlowController.DisableAllPanels();
        backgroundPanel.SetActive(false);
        gameplayPanel.SetActive(true);

        LoadLevel();
        uiFlowController.UpdateHUD(HUDVisibilityController.UIState.Gameplay);
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
        // Wait for player hit animation
        yield return new WaitForSeconds(1.8f);
        // adjust if your animation is longer

        // Stop obstacle movement AFTER they fall
        movementController.OnGameOver();

        // Small buffer for obstacle settle
        yield return new WaitForSeconds(0.3f);
        levelTimeController.StopTimer();
        uiFlowController.ShowGameOver();
    }

    public void Retry()
    {
        levelTimeController.StopTimer();
        // Retry ALWAYS costs battery
        if (!BatteryManager.Instance.HasBattery())
        {
            uiFlowController.ShowNoBatteryPanel(gameplayPanel);
            return;
        }

        BatteryManager.Instance.ConsumeBattery();

        Time.timeScale = 1f;
        StopAllCoroutines();

        snapshot.ClearSnapshot();
        player.ResetPosition();
        layoutIndex = 0;

        uiFlowController.DisableAllPanels();
        backgroundPanel.SetActive(false);
        gameplayPanel.SetActive(true);

        LoadLevel();

        movementController.ResetState();
        //DecideMovementForCurrentLayout();

        uiFlowController.UpdateHUD(HUDVisibilityController.UIState.Gameplay);
    }
    public void StartSnapshot()
    {
        if (snapshotActive || powerUpActive || freezeTimeActive) return;

        JsonLevel level = JsonLevelLoader.Instance.GetLevel(levelIndex);
        snapshotTimer = level.snapshotTime;

        snapshot.TakeSnapshot();
        snapshotActive = true;

        movementController.OnSnapshotStart();   // ✅ NEW

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

            movementController.OnSnapshotEnd();   // ✅ NEW

            UpdatePlayerMovement();
        }
    }
    IEnumerator StartSnapshotNextFrame()
    {
        yield return null; // wait 1 frame (VERY IMPORTANT)

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
                layoutIndex = 0; // loop layouts

            generator.GenerateFromJson(levelIndex, layoutIndex);

            movementController.InitializeLayout(levelIndex);

            snapshot.ClearSnapshot();
            snapshotActive = false;

            // START SNAPSHOT FOR NEW LAYOUT
            StartCoroutine(StartSnapshotNextFrame());
            // reset map timer from JSON
            mapTimer = level.mapChangeTime;
        }
    }
    public void BoosterCollected()
    {
        generator.DestroyAllObstacles();
        generator.DestroyBooster();
    }
    //void UpdateLevelTimer()
    //{
    //    levelTimer -= Time.deltaTime;
    //    timerText.text = levelTimer.ToString("0");

    //    if (levelTimer <= 0)
    //        uiFlowController.ShowGameOver();
    //}
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

        uiFlowController.DisableAllPanels();
        backgroundPanel.SetActive(false);
        gameplayPanel.SetActive(true);

        levelText.text = "LEVEL " + levelIndex;

        gameStateController.SetState(GameStateController.GameState.Gameplay);
        player.canMove = true;
        movementController.ResetState();
        movementController.InitializeLayout(levelIndex);
        uiFlowController.UpdateHUD(HUDVisibilityController.UIState.Gameplay);

    }
    public void OnNewWorldYesClicked()
    {
        if (pendingUnlockedWorld == null)
            return;

        int worldId = pendingUnlockedWorld.worldId;

        PlayerPrefs.SetInt("SelectedWorld", worldId);
        PlayerPrefs.Save();

        pendingUnlockedWorld = null;

        uiFlowController.DisableAllPanels();

        // 🔥 Open Level Panel of that world
        uiFlowController.OpenLevelPanel();

        uiFlowController.UpdateHUD(HUDVisibilityController.UIState.Level);
        gameStateController.SetState(GameStateController.GameState.LevelSelect);
    }
    public void OnNewWorldNoClicked()
    {
        pendingUnlockedWorld = null;

        // Continue normal flow → next level
        OnNextLevelButton();
    }
    public int CurrentLevelNumber
    {
        get { return levelIndex; }
    }

    void ClearLevelFailed(int level)
    {
        PlayerPrefs.DeleteKey($"LevelFailed_{level}");
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
        pendingUnlockedWorld = world;   // 🔥 THIS WAS MISSING

        uiFlowController.DisableAllPanels();
        newWorldPanel.SetActive(true);

        newWorldNameText.text = world.worldName.ToUpper();
        newWorldQuestionText.text = "Do you want to go to this world now?";
    }
}
