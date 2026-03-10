
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

    [Header("Powerup Controller")]
    public PowerUpController powerUpController;

    [Header("Gameplay References")]
    public SnapshotManager snapshot;
    public LevelGenerator generator;
    public PlayerController player;

    [Header("UI - Gameplay")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI mapTimerText;

    public TextMeshProUGUI coinsEarnedText;

    [Header("Level Unlock System")]
    public int totalLevels = 50;

    public int levelIndex = 0;
    private int layoutIndex = 0;

    private float mapTimer;
    private float snapshotTimer;

    private bool snapshotActive;
    private int levelsSinceLastAd = 0;
    private const string SNAPSHOT_KEY = "SNAPSHOT_COUNT";
    private const string SNAPSHOT_INIT_KEY = "SNAPSHOT_INITIALIZED";

    private WorldData pendingUnlockedWorld;

    private bool isTutorial;

    public bool IsSnapshotActive => snapshotActive;

    private const float TUTORIAL_TIME = 30f;
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
            PlayerPrefs.SetInt(SNAPSHOT_KEY, 3); // Level 1 initial gift
            PlayerPrefs.SetInt(SNAPSHOT_INIT_KEY, 1);
            PlayerPrefs.Save();
        }
        isTutorial = PlayerPrefs.GetInt("TutorialDone", 0) == 0;
        if (PlayerPrefs.GetInt("WorldUnlocked_1", 0) == 0)
        {
            PlayerPrefs.SetInt("WorldUnlocked_1", 1);
            PlayerPrefs.Save();
        }
        Time.timeScale = 1f;

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
        CheckTutorial();
    }

    void Update()
    {
        if (!gameStateController.IsGameplayActive()) return;

        if (snapshotActive && !powerUpController.IsAnyPowerUpActive())
            UpdateSnapshotTimer();

        UpdateMapTimer();
    }

    void CheckTutorial()
    {
        // Tutorial runs only first time
        isTutorial = PlayerPrefs.GetInt("TutorialDone", 0) == 0;
    }

    public void UpdatePlayerMovement()
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

        //levelTimeController.StartTimer(level.levelTime);
        //mapTimer = level.mapChangeTime;
        //snapshotTimer = level.snapshotTime;

        if (isTutorial && levelIndex == 1)
        {
            levelTimeController.StartTimer(TUTORIAL_TIME);
            mapTimer = TUTORIAL_TIME;
        }
        else
        {
            levelTimeController.StartTimer(level.levelTime);
            mapTimer = level.mapChangeTime;
        }

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
        powerUpController.UpdatePowerUpUI();

        //for tutorial
        if (levelIndex == 1)
        {
            TutorialManager.Instance.StartTutorial();
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
        // -------- Interstitial Logic --------
        if (levelIndex > 10)
        {
            levelsSinceLastAd++;

            if (levelsSinceLastAd >= 3)
            {
                levelsSinceLastAd = 0;
                AdManager.Instance.ShowInterstitial();
            }
        }
    }

    public void ShowMenuWithAd()
    {
        if (levelIndex > 10)
        {
            AdManager.Instance.ShowInterstitial();
        }

        uiFlowController.ShowMenu();
        gameStateController.SetState(GameStateController.GameState.Menu);
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
        yield return new WaitForSeconds(1.5f);

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
        if (snapshotActive || powerUpController.IsAnyPowerUpActive())
            return;
        player.SetControlInteraction(false);
        JsonLevel level = JsonLevelLoader.Instance.GetLevel(levelIndex);
        snapshotTimer = level.snapshotTime;

        snapshot.TakeSnapshot();
        snapshotActive = true;

        movementController.OnSnapshotStart();
        powerUpController.UpdatePowerUpUI();

        UpdatePlayerMovement();

       // Debug.Log("Snapshot Time = " + snapshotTimer);
    }

    void UpdateSnapshotTimer()
    {
        snapshotTimer -= Time.deltaTime;

        if (snapshotTimer <= 0)
        {
            snapshot.ClearSnapshot();
            snapshotActive = false;

            movementController.OnSnapshotEnd();
            powerUpController.UpdatePowerUpUI();
            player.SetControlInteraction(true);
            UpdatePlayerMovement();

            if (levelIndex == 1)
            {
                TutorialManager.Instance.OnSnapshotFinished();
            }
        }
    }

    IEnumerator StartSnapshotNextFrame()
    {
        yield return null;

        if (!gameStateController.IsGameplayActive()) yield break;

        StartSnapshot();
    }
    public void UseManualSnapshot()
    {
        if (snapshotActive || powerUpController.IsAnyPowerUpActive())
            return;

        if (!ConsumeSnapshot())
            return;
        powerUpController.UpdatePowerUpUI();
        JsonLevel level = JsonLevelLoader.Instance.GetLevel(levelIndex);
        snapshotTimer = level.snapshotTime;

        snapshot.TakeSnapshot();
        snapshotActive = true;

        movementController.OnSnapshotStart();
        powerUpController.UpdatePowerUpUI();
        UpdatePlayerMovement();
    }
    void UpdateMapTimer()
    {
        mapTimer -= Time.deltaTime;
        mapTimerText.text = mapTimer.ToString("0");

        if (mapTimer <= 0)
        {
            if (isTutorial && levelIndex == 1)
            {
                mapTimer = TUTORIAL_TIME;
                mapTimerText.text = mapTimer.ToString("0");
                return;
            }

            player.SnapToTargetTile();
            player.StopMovementImmediately();
            player.ForceStopAnimation();
            player.canMove = false;

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
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnBoosterCollected();
        }
    }

    public void RevivePlayer()
    {
        levelTimeController.StopTimer();
        StopAllCoroutines();

        Time.timeScale = 1f;

        snapshot.ClearSnapshot();
;
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

    public void SetSnapshotInactive()
    {
        snapshotActive = false;
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
    public void AddSnapshotUse()
    {
        int count = GetSnapshotUses();
        PlayerPrefs.SetInt(SNAPSHOT_KEY, count + 1);
        PlayerPrefs.Save();
    }
    public int GetSnapshotUses()
    {
        return PlayerPrefs.GetInt(SNAPSHOT_KEY, 0);
    }
    public bool ConsumeSnapshot()
    {
        int count = GetSnapshotUses();

        if (count <= 0)
            return false;

        PlayerPrefs.SetInt(SNAPSHOT_KEY, count - 1);
        PlayerPrefs.Save();
        return true;
    }
    public void CompleteTutorial()
    {
        isTutorial = false;
    }
}
