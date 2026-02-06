
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManagerCycle : MonoBehaviour
{
    public static GameManagerCycle Instance;

    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject gameplayPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;

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

    //[Header("Levels (ScriptableObjects)")]
    ////public List<LevelData> levels;
    [Header("Power Up")]
    private float freezeTimer;
    private float powerUpTimer;
    public float powerUpDuration = 2f;
    public float freezeTimeDuration = 2f;
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

    private HashSet<MovingObstacle> movingObstaclesForLayout = new HashSet<MovingObstacle>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f;
        LoadHighestLevel();
        ShowMenu();
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
    }

    void UpdatePlayerMovement()
    {
        player.canMove = isGameRunning && !snapshotActive;
    }
    public void ShowMenu()
    {
        DisableAllPanels();
        menuPanel.SetActive(true);

        bestLevelMenuText.text =
            "BEST LEVEL : " + PlayerPrefs.GetInt("HighestLevel", 1);

        isGameRunning = false;
        player.canMove = false;
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        StopAllCoroutines();

        levelIndex = 0;
        layoutIndex = 0;

        snapshot.ClearSnapshot();
        player.ResetPosition();

        DisableAllPanels();
        gameplayPanel.SetActive(true);

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

        levelText.text = "LEVEL " + (levelIndex + 1);

        timerText.text = "TIMER : " + levelTimer.ToString("0");
        mapTimerText.text = "MAP TIMER : " + mapTimer.ToString("0");

        generator.GenerateFromJson(levelIndex + 1, layoutIndex);

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

        bool allowMovement = (levelIndex + 1) >= 4;

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

        if (!allowMovement || obstacles.Count == 0)
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
                obstacles[i].StartWarningGlow();
            else
                obstacles[i].ForceStopMovement();
        }
    }

    //void SetObstacleMovement(bool active)
    //{
    //    foreach (Transform ob in generator.obstaclesParent)
    //    {
    //        MovingObstacle mo = ob.GetComponent<MovingObstacle>();
    //        if (mo != null)
    //            mo.canMove = active;
    //    }
    //}

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

    public void PlayerReachedDoor()
    {
        StartCoroutine(LevelCompleteSequence());
    }

    IEnumerator LevelCompleteSequence()
    {
        isGameRunning = false;
        player.canMove = false;

        DisableAllPanels();
        levelCompletePanel.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        levelIndex++;
        layoutIndex = 0;

        UpdateHighestLevel();

        DisableAllPanels();
        gameplayPanel.SetActive(true);

        LoadLevel();
    }

    public void PlayerHitObstacle()
    {
        StartCoroutine(GameOverDelay());
    }

    IEnumerator GameOverDelay()
    {
        player.PlayHitAnimation();
        yield return new WaitForSeconds(0.8f);
        ShowGameOver();
    }

    public void ShowGameOver()
    {
        DisableAllPanels();
        gameOverPanel.SetActive(true);

        //SetObstacleMovement(false);
        StopAllObstacleMovement();

        PlayerPrefs.SetInt("LastReachedLevel", levelIndex + 1);
        UpdateHighestLevel();

        lastLevelText.text = "LEVEL REACHED : " + (levelIndex + 1);
        bestLevelGameOverText.text =
            "BEST LEVEL : " + PlayerPrefs.GetInt("HighestLevel", 1);

        isGameRunning = false;
        player.canMove = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        StopAllCoroutines();

        levelIndex = 0;
        layoutIndex = 0;

        snapshot.ClearSnapshot();
        //SetObstacleMovement(false);
        StopAllObstacleMovement();

        player.ResetPosition();

        snapshotActive = false;
        isGameRunning = false;

        DisableAllPanels();
        gameplayPanel.SetActive(true);

        LoadLevel();
    }

    public void Retry()
    {
        Time.timeScale = 1f;

        snapshot.ClearSnapshot();
        player.ResetPosition();

        layoutIndex = 0;

        DisableAllPanels();
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
        //SetObstacleMovement(false);
        StopAllObstacleMovement();

        Time.timeScale = 0f;
        isGameRunning = false;
        UpdatePlayerMovement();
    }

    public void ResumeGame()
    {
        DisableAllPanels();
        gameplayPanel.SetActive(true);
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

            JsonLevel level = JsonLevelLoader.Instance.GetLevel(levelIndex+1);

            if (layoutIndex >= level.layouts.Count)
                layoutIndex = 0; // loop layouts

            generator.GenerateFromJson(levelIndex + 1, layoutIndex);

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
        int currentLevel = levelIndex + 1;
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
}
