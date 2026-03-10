using TMPro;
using UnityEngine;

public class LevelTimeController : MonoBehaviour
{
    [Header("Dependencies")]
    public GameStateController gameStateController;
    public UIFlowController uiFlowController;
    public TextMeshProUGUI timerText;

    private float levelTimer;
    private bool isRunning;

    public void StartTimer(float duration)
    {
        levelTimer = duration;
        isRunning = true;
        UpdateUI();
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    void Update()
    {
        if (!isRunning) return;
        if (!gameStateController.IsGameplayActive()) return;

        levelTimer -= Time.deltaTime;

        if (levelTimer <= 0f)
        {
            levelTimer = 0f;
            isRunning = false;
            UpdateUI();

            GameManagerCycle.Instance.HandleTimeOut();
            return;
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        timerText.text = Mathf.CeilToInt(levelTimer).ToString();
    }

    public float GetRemainingTime()
    {
        return levelTimer;
    }

}