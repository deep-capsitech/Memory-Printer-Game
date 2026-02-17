
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

    //[Header("World Level Panels")]
    //public List<GameObject> worldLevelPanels;

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

    [Header("Star System")]
    [Header("Star UI")]
    public UnityEngine.UI.Image star1;
    public UnityEngine.UI.Image star2;
    public UnityEngine.UI.Image star3;

    public Sprite filledStar;
    public Sprite emptyStar;

    public TextMeshProUGUI totalStarsText;

    private int earnedStars;
    private int totalStars;

    //[Header("World Unlock Settings")]
    //public int[] worldUnlockStars = new int[5] { 0, 20, 45, 75, 110 };

    //[Header("World Lock Images")]
    //public GameObject[] worlds;

    //[Header("Lock & Unlock Sprites")]
    //public Sprite[] lockedSprites;
    //public Sprite[] unlockedSprites;

    [Header("Level Unlock System")]
    public int totalLevels = 50;

    [Header("Power Ups")]
    public float powerUpDuration = 2f;
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

    [Header("Revive Panel")]
    public GameObject revivePanel;

    [Header("HUD")]
    public HUDVisibilityController hud;
    [Header("UI Background")]
    public GameObject backgroundPanel;
    [Header("New Dynamic Level Panel")]
    public GameObject levelPanel;

    private HashSet<MovingObstacle> movingObstaclesForLayout = new HashSet<MovingObstacle>();

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
        UpdateTotalStarsUI();
    }

    void Update()
    {
        if (!isGameRunning) return;

        if (powerUpActive)
            UpdatePowerUpTimer();
        if (freezeTimeActive)
            UpdateFreezeTimer();
        if (snapshotActive)
            UpdateSnapshotTimer();

        UpdateMapTimer();
        UpdateLevelTimer();
    }

    void DisableAllPanels()
    {
        menuPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        levelCompletePanel.SetActive(false);
        worldPanel.SetActive(false);
        revivePanel.SetActive(false);

        //foreach (GameObject panel in worldLevelPanels)
        //    panel.SetActive(false);
        if (levelPanel != null)
            levelPanel.SetActive(false);

        backgroundPanel.SetActive(true);
    }
    void UpdateHUD(HUDVisibilityController.UIState state)
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
        
        //UpdateWorldVisuals();
    }

    //public bool IsWorldUnlocked(int worldIndex)
    //{
    //    int totalStars = PlayerPrefs.GetInt("TotalStar", 0);

    //    return totalStars >= worldUnlockStars[worldIndex];
    //}

    //public void UpdateWorldVisuals()
    //{
    //    int totalStars = PlayerPrefs.GetInt("TotalStar", 0);

    //    for (int i = 0; i < worlds.Length; i++)
    //    {
    //        Image img = worlds[i].GetComponent<Image>();

    //        if (img == null)
    //        {
    //            Debug.LogWarning("No Image component found on " + worlds[i].name);
    //            continue;
    //        }

    //        if (totalStars >= worldUnlockStars[i])
    //        {
    //            img.sprite = unlockedSprites[i];
    //        }
    //        else
    //        {
    //            img.sprite = lockedSprites[i];
    //        }
    //    }
    //}

    public void OpenWorldLevels(int worldIndex)
    {
        DisableAllPanels();
        //worldLevelPanels[worldIndex].SetActive(true);
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
        // 🛑 HARD BLOCK IF NO BATTERY
        if (!BatteryManager.Instance.HasBattery())
        {
            Debug.Log("No battery left! Block level start.");
            // TODO: show battery empty popup
            return;
        }

        // 🔋 CONSUME FIRST
        BatteryManager.Instance.ConsumeBattery();

        Time.timeScale = 1f;
        StopAllCoroutines();

        levelIndex = levelNumber;
        layoutIndex = 0;

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
        levelTimer = 60f;
        mapTimer = 20f;
        layoutIndex = 0;

        //if (levelIndex >= levels.Count)
        //{
        //    ShowMenu(); // game completed
        //    return;
        //}

        //LevelData level = levels[levelIndex];

        //levelTimer = level.levelTime;
        //mapTimer = level.mapChangeTime;

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
    }

    void DecideMovementForCurrentLayout()
    {
        movingObstaclesForLayout.Clear();

        //bool allowMovement = (levelIndex) >= 4;

        List<MovingObstacle> obstacles = new List<MovingObstacle>();

        foreach (Transform ob in generator.obstaclesParent)
        {
            MovingObstacle mo = ob.GetComponent<MovingObstacle>();
            if (mo != null)
            {
                mo.ForceStopMovement();
                obstacles.Add(mo);
            }
        }

        if (obstacles.Count == 0)
            return;

        MovingObstacle.MoveType moveType = MovingObstacle.MoveType.None;

        if (levelIndex >= 11 && levelIndex <= 20)
            moveType = MovingObstacle.MoveType.UpDown;

        else if (levelIndex >= 21 && levelIndex <= 30)
            moveType = MovingObstacle.MoveType.LeftRight;

        else if (levelIndex >= 31 && levelIndex <= 50)
            moveType = MovingObstacle.MoveType.Both;

        if (moveType == MovingObstacle.MoveType.None)
            return;

        // shuffle
        for (int i = 0; i < obstacles.Count; i++)
        {
            int r = Random.Range(i, obstacles.Count);
            (obstacles[i], obstacles[r]) = (obstacles[r], obstacles[i]);
        }

        int moveCount = Mathf.Max(1, obstacles.Count / 2);

        for (int i = 0; i < moveCount; i++)
            movingObstaclesForLayout.Add(obstacles[i]);

        for (int i = 0; i < obstacles.Count; i++)
        {
            if (i < moveCount)
            {
                obstacles[i].SetMovementType(moveType);
                obstacles[i].StartWarningGlow();
            }
            else
                obstacles[i].ForceStopMovement();
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

    public void ActivatePowerUp()
    {
        Debug.Log("Power UP Mode On");

        if (powerUpActive) return;

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
        ApplyStoredMovementRules();
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
        Time.timeScale = 1f;
        player.freezeMode = false;
        player.EnableUnscaledAnimation(false);

        //DecideMovementForCurrentLayout();
        ApplyStoredMovementRules();
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

            UpdateTotalStarsUI();
        }
    }

    void ShowStars(int starCount)
    {
        star1.sprite = (starCount >= 1) ? filledStar : emptyStar;
        star2.sprite = (starCount >= 2) ? filledStar : emptyStar;
        star3.sprite = (starCount >= 3) ? filledStar : emptyStar;
    }


    void UpdateTotalStarsUI()
    {
        if (totalStarsText != null)
            totalStarsText.text = "TOTAL STARS : " + totalStars;
    }

    public void PlayerReachedDoor()
    {
        //StartCoroutine(LevelCompleteSequence());
        OnLevelCompleted();
    }

   

    void OnLevelCompleted()
    {
        isGameRunning = false;
        player.canMove = false;

        CalculateStars();
        GiveCoinsForStars();

        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        if (levelIndex >= unlockedLevel && levelIndex < totalLevels)
        {
            PlayerPrefs.SetInt("UnlockedLevel", levelIndex + 1);
            PlayerPrefs.Save();
        }

        DisableAllPanels();
        levelCompletePanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.LevelComplete);
    }
    public void OnNextLevelButton()
    {
        if (!BatteryManager.Instance.HasBattery())
        {
            Debug.Log("No battery left for next level");
            ShowMenu(); // or battery popup
            return;
        }

        BatteryManager.Instance.ConsumeBattery();

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


    //public void PlayerHitObstacle()
    //{
    //    StartCoroutine(GameOverDelay());
    //}

    //IEnumerator GameOverDelay()
    //{
    //    player.PlayHitAnimation();
    //    yield return new WaitForSeconds(0.8f);
    //    ShowGameOver();
    //}

    public void PlayerHitObstacle()
    {
        if (!isGameRunning) return;

        isGameRunning = false;
        player.PlayHitAnimation();

        StartCoroutine(ShowReviveOrGameOver());
    }

    IEnumerator ShowReviveOrGameOver()
    {
        yield return new WaitForSeconds(0.8f);

        DisableAllPanels();
        revivePanel.SetActive(true);   // your existing UI
        UpdateHUD(HUDVisibilityController.UIState.Revive);
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

    //public void RestartGame()
    //{
    //    Time.timeScale = 1f;
    //    StopAllCoroutines();

    //    levelIndex = 0;
    //    layoutIndex = 0;

    //    snapshot.ClearSnapshot();
    //    //SetObstacleMovement(false);
    //    StopAllObstacleMovement();

    //    player.ResetPosition();

    //    snapshotActive = false;
    //    isGameRunning = false;

    //    DisableAllPanels();
    //    gameplayPanel.SetActive(true);

    //    LoadLevel();
    //}

    public void Retry()
    {
        if (!BatteryManager.Instance.HasBattery())
        {
            Debug.Log("No battery left for retry");
            return;
        }

        BatteryManager.Instance.ConsumeBattery();

        Time.timeScale = 1f;

        snapshot.ClearSnapshot();
        player.ResetPosition();

        layoutIndex = 0;

        DisableAllPanels();
        backgroundPanel.SetActive(false);
        gameplayPanel.SetActive(true);

        LoadLevel(); 

        //SetObstacleMovement(false);
        StopAllObstacleMovement() ; 
        DecideMovementForCurrentLayout();

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
        ApplyStoredMovementRules();
        UpdatePlayerMovement();
    }

    public void StartSnapshot()
    {
        if (snapshotActive) return;

        snapshot.TakeSnapshot();
        snapshotTimer = 5f;
        snapshotActive = true;
        UpdatePlayerMovement();
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
            ApplyStoredMovementRules();
        }
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

            mapTimer = 20f;
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

        // ✅ FIX: restore correct HUD (prevents coin/battery UI)
        UpdateHUD(HUDVisibilityController.UIState.Gameplay);
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

}
