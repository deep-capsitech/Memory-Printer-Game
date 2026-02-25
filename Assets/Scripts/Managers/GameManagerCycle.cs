
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerCycle : MonoBehaviour
{
    public static GameManagerCycle Instance;

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

    private int earnedStars;
    private int totalStars;

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
    private int highestLevel = 1;

    private float levelTimer;
    private float mapTimer;
    private float snapshotTimer;

    private bool snapshotActive;
    private bool isGameRunning;

    [Header("HUD")]
    public HUDVisibilityController hud;

    [Header("UI Background")]
    public GameObject backgroundPanel;

    [Header("New Dynamic Level Panel")]
    public GameObject levelPanel;

    private HashSet<MovingObstacle> movingObstaclesForLayout = new HashSet<MovingObstacle>();

    [Header("No Battery Panel")]
    public GameObject noBatteryPanel;

    [Header("New World Unlock Panel")]
    public GameObject newWorldPanel;
    public TextMeshProUGUI newWorldNameText;
    public TextMeshProUGUI newWorldQuestionText;

    private GameObject previousPanelBeforeNoBattery;
    private WorldData pendingUnlockedWorld;
    [Header("Daily Reward")]
    public GameObject dailyRewardPanel;
    [Header("Daily Reward Button")]
    public Button dailyRewardButton;
    public TextMeshProUGUI dailyRewardButtonText;
    public Image dailyRewardButtonIcon;
    private const string DAILY_POPUP_DATE = "DailyRewardPopupDate";
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
        LoadHighestLevel();
        ShowMenu();
        totalStars = PlayerPrefs.GetInt("TotalStar", 0);
    }

    void Update()
    {
        if (!isGameRunning) return;

        if (powerUpActive)
            UpdatePowerUpTimer();
        if (freezeTimeActive)
            UpdateFreezeTimer();
        if (snapshotActive && !freezeTimeActive && !powerUpActive)
            UpdateSnapshotTimer();

        UpdateLevelTimer();
        UpdateMapTimer();
    }

    void DisableAllPanels()
    {
        menuPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        levelCompletePanel.SetActive(false);
        worldPanel.SetActive(false);

        if (levelPanel != null)
            levelPanel.SetActive(false);

        if (noBatteryPanel != null)
            noBatteryPanel.SetActive(false);

        if (newWorldPanel != null)
            newWorldPanel.SetActive(false);

        backgroundPanel.SetActive(true);
    }
    public void UpdateHUD(HUDVisibilityController.UIState state)
    {
        if (hud != null)
            hud.UpdateHUD(state);
    }

    void UpdatePlayerMovement()
    {
        player.canMove = isGameRunning && !snapshotActive;
    }
    public void ShowMenu()
    {
        Time.timeScale = 1f;

        snapshot.ClearSnapshot();
        StopAllCoroutines();

        DisableAllPanels();
        menuPanel.SetActive(true);

        UpdateHUD(HUDVisibilityController.UIState.Menu);

        bestLevelMenuText.text =
            "BEST LEVEL : " + PlayerPrefs.GetInt("HighestLevel", 1);

        isGameRunning = false;
        player.canMove = false;

        // AUTO DAILY REWARD (ONCE PER DAY)
        if (ShouldAutoShowDailyReward())
        {
            DisableAllPanels();
            dailyRewardPanel.SetActive(true);
            UpdateHUD(HUDVisibilityController.UIState.Menu);
            return;
        }

        UpdateDailyRewardButton();
    }
    void UpdateDailyRewardButton()
    {
        if (DailyRewardManager.Instance == null)
            return;

        bool canClaim = DailyRewardManager.Instance.CanShowDailyReward();

        dailyRewardButton.interactable = true; // ALWAYS clickable

        if (canClaim)
        {
            dailyRewardButtonText.text = "CLAIM";
            dailyRewardButtonIcon.color = Color.white; // or yellow glow
        }
        else
        {
            dailyRewardButtonText.text = "CLAIMED";
            dailyRewardButtonIcon.color = Color.white; // NOT gray
        }
    }
    bool ShouldAutoShowDailyReward()
    {
        if (DailyRewardManager.Instance == null)
            return false;

        if (!DailyRewardManager.Instance.CanShowDailyReward())
            return false;

        string lastPopupDate = PlayerPrefs.GetString(DAILY_POPUP_DATE, "");
        string today = System.DateTime.UtcNow.ToString("yyyyMMdd");

        if (lastPopupDate == today)
            return false;

        PlayerPrefs.SetString(DAILY_POPUP_DATE, today);
        PlayerPrefs.Save();

        return true;
    }
    public void OnDailyRewardButtonClicked()
    {
        DisableAllPanels();
        dailyRewardPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.Menu);
    }
    public void OnStartGameClicked()
    {
        Time.timeScale = 1f; 
        StopAllCoroutines();

        snapshot.ClearSnapshot();
        player.ResetPosition();

        DisableAllPanels();
        worldPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.World);
        isGameRunning = false;
        player.canMove = false;

    }
    public void OpenWorldLevels(int worldIndex)
    {
        DisableAllPanels();
        levelPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.Level);
    }

    public void BackToWorldPanel()
    {
        DisableAllPanels();
        worldPanel.SetActive(true);
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
                ShowNoBatteryPanel();
                return;
            }

            BatteryManager.Instance.ConsumeBattery();
        }

        Time.timeScale = 1f;
        StopAllCoroutines();

        snapshot.ClearSnapshot();
        player.ResetPosition();

        DisableAllPanels();
        backgroundPanel.SetActive(false);
        gameplayPanel.SetActive(true);

        UpdateHUD(HUDVisibilityController.UIState.Gameplay);
        LoadLevel();
    }

    void LoadLevel()
    {
        GameEconomyManager.Instance.ResetLevelCoins();

        JsonLevel level = JsonLevelLoader.Instance.GetLevel(levelIndex);

        levelTimer = level.levelTime;
        mapTimer = level.mapChangeTime;
        snapshotTimer = level.snapshotTime;

        layoutIndex = 0;
        
        levelText.text = "LEVEL " + (levelIndex);
        timerText.text = "TIMER : " + levelTimer.ToString("0");
        mapTimerText.text = "MAP TIMER : " + mapTimer.ToString("0");

        generator.GenerateFromJson(levelIndex, layoutIndex);

        //SetObstacleMovement(false); 
        DecideMovementForCurrentLayout();      
        ApplyStoredMovementRules();

        snapshot.ClearSnapshot();
        player.ResetPosition();

        snapshotActive = false;
        isGameRunning = true;
        UpdatePlayerMovement();
        StartCoroutine(StartSnapshotNextFrame());
    }

    void DecideMovementForCurrentLayout()
    {
        movingObstaclesForLayout.Clear();

        //bool allowMovement = (levelIndex) >= 4;

        //List<MovingObstacle> obstacles = new List<MovingObstacle>();
        List<MovingObstacle> allObstacles = new List<MovingObstacle>();
        List<MovingObstacle> eligibleObstacles = new List<MovingObstacle>();

        foreach (Transform ob in generator.obstaclesParent)
        {
            MovingObstacle mo = ob.GetComponent<MovingObstacle>();
            if (mo != null)
            {
                mo.ForceStopMovement();
                allObstacles.Add(mo);
            }
        }

        if (allObstacles.Count == 0)
            return;

        MovingObstacle.MoveType moveType = MovingObstacle.MoveType.None;

        if (levelIndex >= 11 && levelIndex <= 20)
            moveType = MovingObstacle.MoveType.UpDown;

        else if (levelIndex >= 21 && levelIndex <= 30)
            moveType = MovingObstacle.MoveType.LeftRight;

        else if (levelIndex >= 31 && levelIndex <= 40)
            moveType = MovingObstacle.MoveType.Both;

        else if (levelIndex >= 41 && levelIndex <= 50)
            moveType = MovingObstacle.MoveType.Square;

        if (moveType == MovingObstacle.MoveType.None)
            return;

        // FILTER ELIGIBLE OBSTACLES
        foreach (var mo in allObstacles)
        {
            bool valid = true;

            // UpDown cannot use Z borders
            if (moveType == MovingObstacle.MoveType.UpDown)
            {
                if (mo.tileZ == 0 || mo.tileZ == 9)
                    valid = false;
            }

            if (moveType == MovingObstacle.MoveType.LeftRight)
            {
                if (mo.tileX == 0 || mo.tileX == 9)
                    valid = false;
            }

            if (moveType == MovingObstacle.MoveType.Both)
            {
                if (mo.tileX == 0 || mo.tileX == 9 ||
                    mo.tileZ == 0 || mo.tileZ == 9)
                    valid = false;
            }

            if (valid)
                eligibleObstacles.Add(mo);
        }

        if (eligibleObstacles.Count == 0)
            return;

        // shuffle
        for (int i = 0; i < eligibleObstacles.Count; i++)
        {
            int r = Random.Range(i, eligibleObstacles.Count);
            (eligibleObstacles[i], eligibleObstacles[r]) = (eligibleObstacles[r], eligibleObstacles[i]);
        }

        int moveCount = Mathf.Max(1, allObstacles.Count / 2);
        moveCount = Mathf.Min(moveCount, eligibleObstacles.Count);

        for (int i = 0; i < moveCount; i++)
            movingObstaclesForLayout.Add(eligibleObstacles[i]);

        //for (int i = 0; i < obstacles.Count; i++)
        //{
        //    if (i < moveCount)
        //    {
        //        obstacles[i].SetMovementType(moveType);
        //        obstacles[i].StartWarningGlow();
        //    }
        //    else
        //        obstacles[i].ForceStopMovement();
        //}
        foreach (var mo in allObstacles)
        {
            if (movingObstaclesForLayout.Contains(mo))
            {
                mo.SetMovementType(moveType);
                mo.StartWarningGlow();
            }
            else
            {
                mo.ForceStopMovement();
            }
        }
    }

    void ApplyStoredMovementRules()
    {
        foreach (Transform ob in generator.obstaclesParent)
        {
            MovingObstacle mo = ob.GetComponent<MovingObstacle>();
            if (mo == null) continue;

            mo.ForceStopMovement();

            if (movingObstaclesForLayout.Contains(mo))
                mo.StartWarningGlow();
        }
    }
  
    void StopAllObstacleMovement()
    {
        foreach (Transform ob in generator.obstaclesParent)
        {
            MovingObstacle mo = ob.GetComponent<MovingObstacle>();
            if (mo != null)
                mo.ForceStopMovement();
        }
    }

    bool IsObstacleMovementBlocked()
    {
        return snapshotActive || powerUpActive || freezeTimeActive;
    }
    public void ActivatePowerUp()
    {
        Debug.Log("Power UP Mode On");

        if (powerUpActive) return;

        // TOP FREEZE TIME FIRST
        if (freezeTimeActive)
        {
            EndFreezeTime();
        }

        // STOP SNAPSHOT ALSO
        if (snapshotActive)
        {
            snapshot.ClearSnapshot();
            snapshotActive = false;
        }

        powerUpActive = true;
        //snapshotActive = true;
        powerUpTimer = powerUpDuration;

        snapshot.TakeSnapshot();
        //player.canMove = false;
        Time.timeScale = 0f;
        DisableAllPanels();
        backgroundPanel.SetActive(false);
        CameraManager.Instance.EnableTopCamera();
        generator.EnableDragMode(true); // allow dragging

        //SetObstacleMovement(false);
        StopAllObstacleMovement();
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

        //DecideMovementForCurrentLayout();
        if (!IsObstacleMovementBlocked())
            ApplyStoredMovementRules();
        else
            StopAllObstacleMovement();

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
        Debug.Log("Freeze Time Mode On");

        if (freezeTimeActive) return;

        if (snapshotActive)
        {
            snapshot.ClearSnapshot();
            snapshotActive = false;
        }

        // STOP POWERUP IF RUNNING
        if (powerUpActive)
        {
            EndPowerUp();
        }

        freezeTimeActive = true;
        freezeTimer = freezeTimeDuration;

        snapshot.TakeSnapshot();

        Time.timeScale = 0f;
        player.canMove = true;
        player.freezeMode = true;
        player.EnableUnscaledAnimation(true);

        //SetObstacleMovement(false);
        StopAllObstacleMovement();
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

        //DecideMovementForCurrentLayout();
        if (!IsObstacleMovementBlocked())
            ApplyStoredMovementRules();
        else
            StopAllObstacleMovement();
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
    void CalculateStars()
    {
        float timeTaken = 60f - levelTimer;

        if (timeTaken <= 20f)
            earnedStars = 3;
        else if (timeTaken <= 40f)
            earnedStars = 2;
        else
            earnedStars = 1;

        SaveLevelStars();
        ShowStars(earnedStars);
    }

    void SaveLevelStars()
    {
        int currentLevelNumber = levelIndex;

        string levelKey = "LevelStars" + currentLevelNumber;
        int previousBestStars = PlayerPrefs.GetInt(levelKey, 0);

        if (earnedStars > previousBestStars)
        {
            int difference = earnedStars - previousBestStars;

            totalStars += difference;

            PlayerPrefs.SetInt(levelKey, earnedStars);
            PlayerPrefs.SetInt("TotalStar", totalStars);
            PlayerPrefs.Save();
        }
    }

    void ShowStars(int starCount)
    {
        star1.sprite = (starCount >= 1) ? filledStar : emptyStar;
        star2.sprite = (starCount >= 2) ? filledStar : emptyStar;
        star3.sprite = (starCount >= 3) ? filledStar : emptyStar;
    }

    public void PlayerReachedDoor()
    {
        //StartCoroutine(LevelCompleteSequence());
        OnLevelCompleted();
    }
    void OnLevelCompleted()
    {
        ClearLevelFailed(levelIndex);

        isGameRunning = false;
        player.canMove = false;
       // MarkLevelPlayed(levelIndex);

        CalculateStars();
        GiveCoinsForStars();
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        // unlock NEXT level ONLY after WIN
        if (levelIndex == unlockedLevel && levelIndex < totalLevels)
        {
            PlayerPrefs.SetInt("UnlockedLevel", unlockedLevel + 1);
            PlayerPrefs.Save();
        }

        DisableAllPanels();
        levelCompletePanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.LevelComplete);
        CheckForNewWorldUnlock();
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

                DisableAllPanels();
                levelPanel.SetActive(true);
                UpdateHUD(HUDVisibilityController.UIState.Level);

                return; 
            }
        }

        // Normal next-level flow
        levelIndex++;

        JsonLevel level = JsonLevelLoader.Instance.GetLevel(levelIndex);
        if (level == null)
        {
            ShowMenu();
            return;
        }

        layoutIndex = 0;
        UpdateHighestLevel();

        DisableAllPanels();
        backgroundPanel.SetActive(false);
        gameplayPanel.SetActive(true);

        LoadLevel();
        UpdateHUD(HUDVisibilityController.UIState.Gameplay);
    }
    public void PlayerHitObstacle()
    {
        if (!isGameRunning) return;

        isGameRunning = false;
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
        StopAllObstacleMovement();

        // Small buffer for obstacle settle
        yield return new WaitForSeconds(0.3f);

        ShowGameOver();
    }

    void ShowNoBatteryPanel()
    {
        StopAllCoroutines();

        snapshot.ClearSnapshot();
        freezeTimeActive = false;
        powerUpActive = false;
        snapshotActive = false;

        Time.timeScale = 0f;

        //Store which panel was active
        if (gameOverPanel.activeSelf)
            previousPanelBeforeNoBattery = gameOverPanel;
        else if (levelPanel.activeSelf)
            previousPanelBeforeNoBattery = levelPanel;
        else if (worldPanel.activeSelf)
            previousPanelBeforeNoBattery = worldPanel;
        else
            previousPanelBeforeNoBattery = menuPanel;

        DisableAllPanels();
        noBatteryPanel.SetActive(true);

        UpdateHUD(HUDVisibilityController.UIState.NoBattery);

        isGameRunning = false;
        player.canMove = false;
    }

    public void ReturnFromNoBatteryPanel()
    {
        Time.timeScale = 1f;

        DisableAllPanels();

        if (previousPanelBeforeNoBattery != null)
            previousPanelBeforeNoBattery.SetActive(true);
        else
            menuPanel.SetActive(true);

        // Restore correct HUD
        if (previousPanelBeforeNoBattery == gameOverPanel)
            UpdateHUD(HUDVisibilityController.UIState.GameOver);
        else if (previousPanelBeforeNoBattery == levelPanel)
            UpdateHUD(HUDVisibilityController.UIState.Level);
        else if (previousPanelBeforeNoBattery == worldPanel)
            UpdateHUD(HUDVisibilityController.UIState.World);
        else
            UpdateHUD(HUDVisibilityController.UIState.Menu);
    }

    public void ShowGameOver()
    {
        DisableAllPanels();
        gameOverPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.GameOver);
        //SetObstacleMovement(false);
        StopAllObstacleMovement();

        PlayerPrefs.SetInt("LastReachedLevel", levelIndex);
        UpdateHighestLevel();

        lastLevelText.text = "LEVEL REACHED : " + (levelIndex);
        bestLevelGameOverText.text =
            "BEST LEVEL : " + PlayerPrefs.GetInt("HighestLevel", 1);

        isGameRunning = false;
        player.canMove = false;
    }

    public void Retry()
    {
        // Retry ALWAYS costs battery
        if (!BatteryManager.Instance.HasBattery())
        {
            ShowNoBatteryPanel();
            return;
        }

        BatteryManager.Instance.ConsumeBattery();

        Time.timeScale = 1f;
        StopAllCoroutines();

        snapshot.ClearSnapshot();
        player.ResetPosition();
        layoutIndex = 0;

        DisableAllPanels();
        backgroundPanel.SetActive(false);
        gameplayPanel.SetActive(true);

        LoadLevel();

        StopAllObstacleMovement();
        DecideMovementForCurrentLayout();

        UpdateHUD(HUDVisibilityController.UIState.Gameplay);
    }

    public void PauseGame()
    {
        DisableAllPanels();
        pausePanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.Pause);
        //SetObstacleMovement(false);
        StopAllObstacleMovement();

        Time.timeScale = 0f;
        isGameRunning = false;
        UpdatePlayerMovement();
    }

    public void ResumeGame()
    {
        DisableAllPanels();
        backgroundPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.Gameplay);
        Time.timeScale = 1f;
        isGameRunning = true;
        //ApplyStoredMovementRules();
        if (!snapshotActive && !powerUpActive && !freezeTimeActive)
        {
            ApplyStoredMovementRules();
        }
        else
        {
            StopAllObstacleMovement();
        }
        UpdatePlayerMovement();
        
    }

    public void StartSnapshot()
    {
        if (snapshotActive || powerUpActive || freezeTimeActive) return;
        JsonLevel level = JsonLevelLoader.Instance.GetLevel(levelIndex);
        snapshotTimer = level.snapshotTime;
        snapshot.TakeSnapshot();
        snapshotActive = true;
        StopAllObstacleMovement();
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
            //player.canMove = true;
            UpdatePlayerMovement();
            //DecideMovementForCurrentLayout();
            if (!freezeTimeActive && !powerUpActive)
            {
                ApplyStoredMovementRules();
            }
            else
            {
                StopAllObstacleMovement();
            }
        }
    }

    IEnumerator StartSnapshotNextFrame()
    {
        yield return null; // wait 1 frame (VERY IMPORTANT)

        if (!isGameRunning) yield break;

        StartSnapshot();
    }
    void UpdateMapTimer()
    {
        mapTimer -= Time.deltaTime;
        mapTimerText.text = "MAP TIMER : " + mapTimer.ToString("0");

        if (mapTimer <= 0)
        {
            layoutIndex++;

            JsonLevel level = JsonLevelLoader.Instance.GetLevel(levelIndex);

            if (layoutIndex >= level.layouts.Count)
                layoutIndex = 0; // loop layouts

            generator.GenerateFromJson(levelIndex, layoutIndex);

            DecideMovementForCurrentLayout();
            //SetObstacleMovement(false);
            StopAllObstacleMovement();

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
    void UpdateLevelTimer()
    {
        levelTimer -= Time.deltaTime;
        timerText.text = "TIMER : " + levelTimer.ToString("0");

        if (levelTimer <= 0)
            ShowGameOver();
    }

    void LoadHighestLevel()
    {
        highestLevel = PlayerPrefs.GetInt("HighestLevel", 1);
    }

    void SaveHighestLevel()
    {
        PlayerPrefs.SetInt("HighestLevel", highestLevel);
        PlayerPrefs.Save();
    }

    void UpdateHighestLevel()
    {
        int currentLevel = levelIndex;
        if (currentLevel > highestLevel)
        {
            highestLevel = currentLevel;
            SaveHighestLevel();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GiveCoinsForStars()
    {
        int coins = 0;

        switch (earnedStars)
        {
            case 3:
                coins = 15;
                break;
            case 2:
                coins = 10;
                break;
            case 1:
                coins = 5;
                break;
        }

        GameEconomyManager.Instance.AddCoins(coins);
    }

    public void RevivePlayer()
    {
        StopAllCoroutines();

        Time.timeScale = 1f;

        snapshot.ClearSnapshot();
        freezeTimeActive = false;
        powerUpActive = false;
        snapshotActive = false;

        player.ReviveToLastSafeTile();

        DisableAllPanels();
        backgroundPanel.SetActive(false);
        gameplayPanel.SetActive(true);

        levelText.text = "LEVEL " + levelIndex;

        isGameRunning = true;
        player.canMove = true;

        ApplyStoredMovementRules();

        // FIX: restore correct HUD (prevents coin/battery UI)
        UpdateHUD(HUDVisibilityController.UIState.Gameplay);
    }
    public void OnNewWorldYesClicked()
    {
        if (pendingUnlockedWorld == null)
        {
            Debug.LogWarning("No pending world to open.");
            return;
        }

        PlayerPrefs.SetInt("SelectedWorld", pendingUnlockedWorld.worldId);
        PlayerPrefs.Save();

        OpenWorldLevels(pendingUnlockedWorld.worldId);

        pendingUnlockedWorld = null;
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

    public void OpenLevelPanel()
    {
        DisableAllPanels();
        levelPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.Level);
    }

    void ClearLevelFailed(int level)
    {
        PlayerPrefs.DeleteKey($"LevelFailed_{level}");
    }

    bool IsWorldUnlocked(int worldId)
    {
        return PlayerPrefs.GetInt($"WorldUnlocked_{worldId}", 0) == 1;
    }

    void UnlockWorld(int worldId)
    {
        PlayerPrefs.SetInt($"WorldUnlocked_{worldId}", 1);
        PlayerPrefs.Save();
    }

    bool IsWorldUnlockPopupShown(int worldId)
    {
        return PlayerPrefs.GetInt($"WorldUnlockPopupShown_{worldId}", 0) == 1;
    }

    void MarkWorldUnlockPopupShown(int worldId)
    {
        PlayerPrefs.SetInt($"WorldUnlockPopupShown_{worldId}", 1);
        PlayerPrefs.Save();
    }

    void ShowNewWorldUnlockedPanel(WorldData world)
    {
        DisableAllPanels();
        newWorldPanel.SetActive(true);

        pendingUnlockedWorld = world;

        // ONLY world name here
        newWorldNameText.text = world.worldName.ToUpper();

        // Question is FIXED text
        newWorldQuestionText.text = "Do you want to go to this world now?";
    }

    void CheckForNewWorldUnlock()
    {
        int totalStars = PlayerPrefs.GetInt("TotalStar", 0);

        foreach (WorldData world in WorldDatabase.Instance.GetWorlds())
        {
            // World 1 is always unlocked by default → never show popup
            if (world.worldId == 1)
                continue;

            // Skip if already unlocked
            if (IsWorldUnlocked(world.worldId))
                continue;

            // Requirement not met yet
            if (totalStars < world.starsRequired)
                continue;

            // Unlock the world
            UnlockWorld(world.worldId);
            int levelsPerWorld = 10;
            int firstLevelOfWorld = (world.worldId - 1) * levelsPerWorld + 1;

            int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
            if (unlockedLevel < firstLevelOfWorld)
            {
                PlayerPrefs.SetInt("UnlockedLevel", firstLevelOfWorld);
                PlayerPrefs.Save();
            }
            // Show popup ONLY ONCE
            if (!IsWorldUnlockPopupShown(world.worldId))
            {
                MarkWorldUnlockPopupShown(world.worldId);
                ShowNewWorldUnlockedPanel(world);
            }

            break; // unlock ONLY one world at a time
        }
    }
}
