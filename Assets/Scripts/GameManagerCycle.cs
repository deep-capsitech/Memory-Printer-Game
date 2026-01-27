
//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.SceneManagement;

//public class GameManagerCycle : MonoBehaviour
//{
//    [Header("Scene Mode")]
//    public bool isGameplayScene = true;

//    [Header("References (Gameplay Only)")]
//    public SnapshotManager snapshot;
//    public LevelGenerator generator;
//    public PlayerController player;
//    public GameUI ui;

//    [Header("Pause UI")]
//    public GameObject pausePanel;

//    [Header("Base Difficulty")]
//    public float baseLevelTimer = 60f;
//    public float baseMapChangeDelay = 30f;
//    public int baseObstacleCount = 10;

//    private float snapshotTimer;
//    private float mapTimer;
//    private float levelTimer;

//    private bool snapshotActive = false;
//    public int level = 1;
//    private bool isGameRunning = true;

//    [Header("UI Effects")]
//    public CanvasGroup levelCompletedPanel;
//    public float levelCompleteFade = 2f;

//    private int highestLevel = 1;

//    void Start()
//    {
//        LoadHighestLevel();

//        if (!isGameplayScene)
//            return;

//        Time.timeScale = 1f;

//        if (pausePanel != null)
//            pausePanel.SetActive(false);

//        StartLevel();
//    }

//    void Update()
//    {
//        if (!isGameplayScene || !isGameRunning) return;

//        HandleSnapshotInput();

//        if (snapshotActive)
//            UpdateSnapshotTimer();

//        UpdateMapTimer();
//        UpdateLevelTimer();
//    }

//    public void PauseGame()
//    {
//        if (!isGameplayScene || !isGameRunning) return;

//        Time.timeScale = 0f;
//        isGameRunning = false;
//        player.canMove = false;

//        if (pausePanel != null)
//            pausePanel.SetActive(true);
//    }

//    public void ResumeGame()
//    {
//        Time.timeScale = 1f;
//        isGameRunning = true;
//        player.canMove = true;

//        if (pausePanel != null)
//            pausePanel.SetActive(false);
//    }

//    public void RestartGame()
//    {
//        Time.timeScale = 1f;
//        SceneManager.LoadScene("Main");
//    }

//    void HandleSnapshotInput()
//    {
//#if UNITY_EDITOR || UNITY_STANDALONE
//        if (Input.GetKeyDown(KeyCode.F))
//            StartSnapshot();
//#endif
//    }

//    public void SnapshotButtonPressed()
//    {
//        StartSnapshot();
//    }

//    void StartSnapshot()
//    {
//        if (snapshotActive) return;

//        snapshot.TakeSnapshot();
//        player.canMove = false;

//        snapshotActive = true;
//        snapshotTimer = 4f;

//        SetObstacleMovement(false);
//    }

//    void UpdateSnapshotTimer()
//    {
//        snapshotTimer -= Time.deltaTime;

//        if (snapshotTimer <= 0)
//            EndSnapshot();
//    }

//    void EndSnapshot()
//    {
//        snapshotActive = false;
//        snapshot.ClearSnapshot();
//        player.canMove = true;

//        ApplyMovementRules();
//    }

//    void UpdateMapTimer()
//    {
//        mapTimer -= Time.deltaTime;
//        ui.UpdateMapTimer(mapTimer);

//        if (mapTimer <= 0)
//        {
//            generator.GenerateNewLayout();
//            mapTimer = GetMapChangeTime();
//        }
//    }

//    void UpdateLevelTimer()
//    {
//        levelTimer -= Time.deltaTime;
//        ui.UpdateLevelTimer(levelTimer);

//        if (levelTimer <= 0)
//            GameOver();
//    }

//    void StartLevel()
//    {
//        generator.obstacleCount = GetObstacleCount();
//        generator.GenerateNewLayout();

//        levelTimer = GetLevelTime();
//        mapTimer = GetMapChangeTime();

//        ui.UpdateLevel(level);
//        ui.UpdateLevelTimer(levelTimer);
//        ui.UpdateMapTimer(mapTimer);

//        isGameRunning = true;
//        player.canMove = true;
//        snapshotActive = false;

//        SetObstacleMovement(false);
//    }

//    public void PlayerReachedDoor()
//    {
//        LevelComplete();
//    }

//    public void LevelComplete()
//    {
//        isGameRunning = false;
//        player.canMove = false;

//        snapshot.ClearSnapshot();
//        player.PlayWinJumpAnimation();
//        UpdateHighestLevel();

//        StartCoroutine(LevelCompleteSequence());
//    }

//    IEnumerator LevelCompleteSequence()
//    {
//        yield return new WaitForSeconds(1.5f);
//        StartCoroutine(ShowLevelCompleteEffect());
//        StartCoroutine(LevelBreak());
//    }

//    IEnumerator LevelBreak()
//    {
//        yield return new WaitForSeconds(1.2f);

//        level++;
//        player.ResetPosition();

//        StartLevel();
//    }

//    public void PlayerHitObstacle()
//    {
//        player.PlayHitAnimation();
//        StartCoroutine(GameOverAfterHit());
//    }

//    IEnumerator GameOverAfterHit()
//    {
//        yield return new WaitForSeconds(0.8f);
//        GameOver();
//    }

//    public void GameOver()
//    {
//        if (!isGameplayScene) return;

//        PlayerPrefs.SetInt("LastReachedLevel", level);
//        PlayerPrefs.Save();

//        isGameRunning = false;
//        player.canMove = false;

//        snapshot.ClearSnapshot();

//        StartCoroutine(GoToGameOver());
//    }

//    IEnumerator GoToGameOver()
//    {
//        yield return new WaitForSeconds(1f);
//        SceneManager.LoadScene("GameOver");
//    }

//    void ApplyMovementRules()
//    {
//        bool allowMovement = level >= 4;

//        List<MovingObstacle> list = new List<MovingObstacle>();

//        foreach (Transform ob in generator.obstaclesParent)
//        {
//            MovingObstacle mo = ob.GetComponent<MovingObstacle>();
//            if (mo != null)
//            {
//                mo.ForceStopMovement();
//                list.Add(mo);
//            }
//        }

//        if (!allowMovement || list.Count == 0) return;

//        int total = list.Count;
//        int moveCount = Mathf.Max(1, total / 2);

//        for (int i = 0; i < list.Count; i++)
//        {
//            int r = Random.Range(i, list.Count);
//            (list[i], list[r]) = (list[r], list[i]);
//        }

//        for (int i = 0; i < list.Count; i++)
//        {
//            if (i < moveCount)
//                list[i].StartWarningGlow();
//            else
//                list[i].ForceStopMovement();
//        }
//    }

//    void SetObstacleMovement(bool active)
//    {
//        foreach (Transform ob in generator.obstaclesParent)
//        {
//            MovingObstacle mo = ob.GetComponent<MovingObstacle>();
//            if (mo != null)
//                mo.canMove = active;
//        }
//    }

//    float GetLevelTime() => Mathf.Max(15f, baseLevelTimer - (level - 1) * 3f);
//    float GetMapChangeTime() => Mathf.Max(8f, baseMapChangeDelay - (level - 1));
//    int GetObstacleCount() => baseObstacleCount + (level - 1) * 2;

//    IEnumerator ShowLevelCompleteEffect()
//    {
//        if (levelCompletedPanel == null) yield break;

//        levelCompletedPanel.gameObject.SetActive(true);
//        levelCompletedPanel.alpha = 0;

//        while (levelCompletedPanel.alpha < 1)
//        {
//            levelCompletedPanel.alpha += Time.deltaTime * levelCompleteFade;
//            yield return null;
//        }

//        yield return new WaitForSeconds(1f);

//        while (levelCompletedPanel.alpha > 0)
//        {
//            levelCompletedPanel.alpha -= Time.deltaTime * levelCompleteFade;
//            yield return null;
//        }

//        levelCompletedPanel.gameObject.SetActive(false);
//    }

//    void LoadHighestLevel()
//    {
//        highestLevel = PlayerPrefs.GetInt("HighestLevel", 1);
//    }

//    void SaveHighestLevel()
//    {
//        PlayerPrefs.SetInt("HighestLevel", highestLevel);
//        PlayerPrefs.Save();
//    }

//    void UpdateHighestLevel()
//    {
//        if (level > highestLevel)
//        {
//            highestLevel = level;
//            SaveHighestLevel();
//        }
//    }

//    public void MainMenu()
//    {
//        SceneManager.LoadScene("Menu");
//    }
//}




//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;

//public class GameManagerCycle : MonoBehaviour
//{
//    public static GameManagerCycle Instance;

//    [Header("Panels")]
//    public GameObject menuPanel;
//    public GameObject gameplayPanel;
//    public GameObject pausePanel;
//    public GameObject gameOverPanel;
//    public GameObject levelCompletePanel;

//    [Header("Gameplay References")]
//    public SnapshotManager snapshot;
//    public LevelGenerator generator;
//    public PlayerController player;
//    //public GameUI ui;

//    [Header("Base Difficulty")]
//    public float baseLevelTimer = 60f;
//    public float baseMapChangeDelay = 30f;
//    public int baseObstacleCount = 10;

//    private float snapshotTimer;
//    private float mapTimer;
//    private float levelTimer;

//    private bool snapshotActive;
//    private bool isGameRunning;
//    public int level = 1;
//    private int highestLevel = 1;
//    private bool isRetrying = false;

//    [Header("Menu UI")]
//    public TextMeshProUGUI bestLevelMenuText;

//    [Header("Gameplay Panel UI")]
//    public TextMeshProUGUI levelText;
//    public TextMeshProUGUI timerText;
//    public TextMeshProUGUI mapTimerText;

//    [Header("Game Over UI")]
//    public TextMeshProUGUI lastLevelText;
//    public TextMeshProUGUI bestLevelGameOverText;

//    void Awake()
//    {
//        if (Instance == null) Instance = this;
//        else Destroy(gameObject);
//    }

//    void Start()
//    {
//        Time.timeScale = 1f;
//        LoadHighestLevel();
//        ShowMenu();
//    }

//    void Update()
//    {
//        if (!isGameRunning) return;

//        if (snapshotActive)
//            UpdateSnapshotTimer();

//        UpdateMapTimer();
//        UpdateLevelTimer();
//    }

//    // UI STATES 

//    void DisableAllPanels()
//    {
//        menuPanel.SetActive(false);
//        gameplayPanel.SetActive(false);
//        pausePanel.SetActive(false);
//        gameOverPanel.SetActive(false);
//        levelCompletePanel.SetActive(false);
//    }

//    public void ShowMenu()
//    {
//        DisableAllPanels();
//        menuPanel.SetActive(true);

//        bestLevelMenuText.text = "BEST LEVEL : " +
//           PlayerPrefs.GetInt("HighestLevel", 1);

//        isGameRunning = false;
//        player.canMove = false;
//    }

//    public void StartGame()
//    {
//        //DisableAllPanels();
//        //gameplayPanel.SetActive(true);

//        //level = 1;
//        //StartLevel();

//        Time.timeScale = 1f;
//        StopAllCoroutines();

//        isGameRunning = false;
//        snapshotActive = false;

//        if (snapshot != null)
//            snapshot.ClearSnapshot();

//        level = 1;
//        levelTimer = 0f;
//        mapTimer = 0f;


//        player.ResetPosition();
//        player.canMove = false;

//        DisableAllPanels();
//        gameplayPanel.SetActive(true);

//        generator.GenerateNewLayout();
//        StartLevel();
//    }

//    public void PauseGame()
//    {
//        DisableAllPanels();
//        pausePanel.SetActive(true);
//        Time.timeScale = 0f;
//        isGameRunning = false;
//        player.canMove = false;
//    }

//    public void ResumeGame()
//    {
//        DisableAllPanels();
//        gameplayPanel.SetActive(true);
//        Time.timeScale = 1f;
//        isGameRunning = true;
//        player.canMove = true;
//    }

//    public void RestartGame()
//    {
//        level = 1;
//        Time.timeScale = 1f;
//        snapshotActive = false;
//        isGameRunning = false;
//        if (snapshot != null)
//            snapshot.ClearSnapshot();
//        player.ResetPosition();
//        SetObstacleMovement(false);
//        DisableAllPanels();
//        gameplayPanel.SetActive(true);
//        StartLevel();
//    }



//    public void ShowGameOver()
//    {
//        DisableAllPanels();
//        gameOverPanel.SetActive(true);

//        PlayerPrefs.SetInt("LastReachedLevel", level);
//        UpdateHighestLevel();

//        lastLevelText.text = "LEVEL REACHED : " + level;
//        bestLevelGameOverText.text = "BEST LEVEL : " +
//            PlayerPrefs.GetInt("HighestLevel", 1);

//        isGameRunning = false;
//        player.canMove = false;
//    }

//    // GAME FLOW

//    void StartLevel()
//    {
//        generator.obstacleCount = GetObstacleCount();

//        if (!isRetrying)
//        {
//            generator.GenerateNewLayout();
//        }
//        else
//        {
//            generator.RestoreLayout();
//            isRetrying = false;
//        }

//        levelTimer = GetLevelTime();
//        mapTimer = GetMapChangeTime();

//        levelText.text = "LEVEL " + level;
//        timerText.text = "TIMER : " + levelTimer.ToString("0");
//        mapTimerText.text = "MAP TIMER : " + mapTimer.ToString("0");

//        //ui.UpdateLevel(level);
//        //ui.UpdateLevelTimer(levelTimer);
//        //ui.UpdateMapTimer(mapTimer);

//        snapshotActive = false;
//        SetObstacleMovement(false);
//        isGameRunning = true;
//        player.canMove = true;
//    }

//    public void PlayerReachedDoor()
//    {
//        StartCoroutine(LevelCompleteSequence());
//    }

//    IEnumerator LevelCompleteSequence()
//    {
//        isGameRunning = false;
//        player.canMove = false;

//        DisableAllPanels();
//        levelCompletePanel.SetActive(true);

//        yield return new WaitForSeconds(1.5f);
//        UpdateHighestLevel();

//        DisableAllPanels();
//        gameplayPanel.SetActive(true);

//        level++;
//        player.ResetPosition();

//        yield return new WaitForSeconds(0.1f);
//        StartLevel();
//    }

//    public void PlayerHitObstacle()
//    {
//        StartCoroutine(GameOverDelay());
//    }

//    IEnumerator GameOverDelay()
//    {
//        player.PlayHitAnimation();
//        yield return new WaitForSeconds(0.8f);
//        ShowGameOver();
//    }

//    // SNAPSHOT

//    public void StartSnapshot()
//    {
//        if (snapshotActive) return;

//        snapshot.TakeSnapshot();
//        snapshotTimer = 4f;
//        snapshotActive = true;
//        player.canMove = false;
//    }

//    void UpdateSnapshotTimer()
//    {
//        snapshotTimer -= Time.deltaTime;
//        if (snapshotTimer <= 0)
//        {
//            snapshot.ClearSnapshot();
//            snapshotActive = false;
//            player.canMove = true;

//            ApplyMovementRules();
//        }
//    }

//    // TIMERS 

//    void UpdateMapTimer()
//    {
//        mapTimer -= Time.deltaTime;
//        //ui.UpdateMapTimer(mapTimer);

//        mapTimerText.text = "MAP TIMER : " + mapTimer.ToString("0");

//        if (mapTimer <= 0)
//        {
//            generator.GenerateNewLayout();
//            mapTimer = GetMapChangeTime();
//        }
//    }

//    void UpdateLevelTimer()
//    {
//        levelTimer -= Time.deltaTime;
//        //ui.UpdateLevelTimer(levelTimer);

//        timerText.text = "TIMER : " + levelTimer.ToString("0");

//        if (levelTimer <= 0)
//            ShowGameOver();
//    }

//    float GetLevelTime() => Mathf.Max(15f, baseLevelTimer - (level - 1) * 3f);
//    float GetMapChangeTime() => Mathf.Max(8f, baseMapChangeDelay - (level - 1));
//    int GetObstacleCount() => baseObstacleCount + (level - 1) * 2;

//    void ApplyMovementRules()
//    {
//        bool allowMovement = level >= 4;

//        List<MovingObstacle> list = new List<MovingObstacle>();

//        foreach (Transform ob in generator.obstaclesParent)
//        {
//            MovingObstacle mo = ob.GetComponent<MovingObstacle>();
//            if (mo != null)
//            {
//                mo.ForceStopMovement();
//                list.Add(mo);
//            }
//        }

//        if (!allowMovement || list.Count == 0) return;

//        int total = list.Count;
//        int moveCount = Mathf.Max(1, total / 2);

//        for (int i = 0; i < list.Count; i++)
//        {
//            int r = Random.Range(i, list.Count);
//            (list[i], list[r]) = (list[r], list[i]);
//        }

//        for (int i = 0; i < list.Count; i++)
//        {
//            if (i < moveCount)
//                list[i].StartWarningGlow();
//            else
//                list[i].ForceStopMovement();
//        }
//    }

//    void SetObstacleMovement(bool active)
//    {
//        foreach (Transform ob in generator.obstaclesParent)
//        {
//            MovingObstacle mo = ob.GetComponent<MovingObstacle>();
//            if (mo != null)
//                mo.canMove = active;
//        }
//    }

//    void LoadHighestLevel()
//    {
//        highestLevel = PlayerPrefs.GetInt("HighestLevel", 1);
//    }

//    void SaveHighestLevel()
//    {
//        PlayerPrefs.SetInt("HighestLevel", highestLevel);
//        PlayerPrefs.Save();
//    }

//    void UpdateHighestLevel()
//    {
//        if (level > highestLevel)
//        {
//            highestLevel = level;
//            SaveHighestLevel();
//        }
//    }

//    public void Retry()
//    {
//        Time.timeScale = 1f;

//        snapshotActive = false;
//        isGameRunning = false;

//        player.ResetPosition();
//        snapshot.ClearSnapshot();

//        DisableAllPanels();
//        gameplayPanel.SetActive(true);

//        isRetrying = true;
//        StartLevel();

//        //levelTimer = GetLevelTime();
//        //mapTimer = GetMapChangeTime();

//        //timerText.text = "TIMER : " + levelTimer.ToString("0");
//        //mapTimerText.text = "MAP TIMER : " + mapTimer.ToString("0");

//        //player.canMove = true;
//        //isGameRunning = true;
//    }

//    public void QuitGame()
//    {
//        Debug.Log("Quit Game..!");
//        Application.Quit();
//    }

//}




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
    public float powerUpDuration = 5f;
    private bool powerUpActive = false;

    private int levelIndex = 0;
    private int layoutIndex = 0;
    private int highestLevel = 1;

    private float levelTimer;
    private float mapTimer;
    private float snapshotTimer;

    private bool snapshotActive;
    private bool isGameRunning;

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
        //timerText.text = "TIMER : 60" ;
        //mapTimerText.text = "MAP TIMER : 20" ;

        timerText.text = "TIMER : " + levelTimer.ToString("0");
        mapTimerText.text = "MAP TIMER : " + mapTimer.ToString("0");

        generator.GenerateFromJson(levelIndex + 1, layoutIndex);

        SetObstacleMovement(false); 
        ApplyMovementRules();       

        snapshot.ClearSnapshot();
        player.ResetPosition();

        snapshotActive = false;
        isGameRunning = true;
        UpdatePlayerMovement();
    }

    void ApplyMovementRules()
    {
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

        for (int i = 0; i < obstacles.Count; i++)
        {
            if (i < moveCount)
                obstacles[i].StartWarningGlow();
            else
                obstacles[i].ForceStopMovement();
        }
    }

    void SetObstacleMovement(bool active)
    {
        foreach (Transform ob in generator.obstaclesParent)
        {
            MovingObstacle mo = ob.GetComponent<MovingObstacle>();
            if (mo != null)
                mo.canMove = active;
        }
    }

    public void ActivatePowerUp()
    {
        Debug.Log("Power UP Mode On");

        if (powerUpActive) return;

        powerUpActive = true;
        snapshotActive = true;
        snapshotTimer = powerUpDuration;

        snapshot.TakeSnapshot();
        //player.canMove = false;

        CameraManager.Instance.EnableTopCamera();
        generator.EnableDragMode(true); // allow dragging

        SetObstacleMovement(false);
        UpdatePlayerMovement();
    }

    void EndPowerUp()
    {
        powerUpActive = false;
        snapshotActive = false;

        snapshot.ClearSnapshot();
        //player.canMove = true;

        CameraManager.Instance.EnableMainCamera();
        generator.EnableDragMode(false);

        ApplyMovementRules();
        UpdatePlayerMovement();
    }

    void UpdatePowerUpTimer()
    {
        if (!powerUpActive) return;

        snapshotTimer -= Time.deltaTime;

        if (snapshotTimer <= 0f)
        {
            EndPowerUp();
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

        SetObstacleMovement(false);

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
        SetObstacleMovement(false);

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

        SetObstacleMovement(false);
        ApplyMovementRules();

    }

    public void PauseGame()
    {
        DisableAllPanels();
        pausePanel.SetActive(true);
        SetObstacleMovement(false);

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
            ApplyMovementRules();
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

            ApplyMovementRules();
            SetObstacleMovement(false);

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
