using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LevelTimeController : MonoBehaviour
{
    [Header("Dependencies")]
    public GameStateController gameStateController;
    public UIFlowController uiFlowController;
    public TextMeshProUGUI timerText;

    private float levelTimer;
    private bool isRunning;
    public Image timerRing;
    private float startDuration;
    public void StartTimer(float duration)
    {
        levelTimer = duration;
        startDuration = duration;
        isRunning = true;

        if (timerRing != null)
            timerRing.fillAmount = 0;

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
        int minutes = Mathf.FloorToInt(levelTimer / 60f);
        int seconds = Mathf.FloorToInt(levelTimer % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (timerRing != null)
        {
            float progress = 1f - (levelTimer / startDuration);
            timerRing.fillAmount = progress;
        }
    }

    public float GetRemainingTime()
    {
        return levelTimer;
    }

}