using UnityEngine;
using TMPro;
using System.Collections;

public class ContinueManager : MonoBehaviour
{
    public static ContinueManager Instance;

    [Header("UI")]
    public GameObject continuePanel;
    public TextMeshProUGUI countdownText;

    [Header("Config")]
    public float countdownDuration = 10f;
    public int coinCost = 100;

    private float timer;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowContinue()
    {
        Debug.Log("CONTINUE PANEL SHOWN");

        // Enable UI FIRST
        continuePanel.SetActive(true);

        // Wait one frame so UI is rendered
        StartCoroutine(BeginContinue());
    }

    IEnumerator BeginContinue()
    {
        yield return null; // critical

        Time.timeScale = 0f; // pause AFTER UI appears

        timer = countdownDuration;
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        while (timer > 0f)
        {
            countdownText.text = Mathf.CeilToInt(timer).ToString();
            timer -= Time.unscaledDeltaTime;
            yield return null;
        }

        ForceGameOver();
    }

    void ForceGameOver()
    {
        continuePanel.SetActive(false);
        Time.timeScale = 1f;

        GameManagerCycle.Instance.ShowGameOver();
    }

    public void OnSpendCoinsContinue()
    {
        if (!GameEconomyManager.Instance.HasEnoughCoins(coinCost))
            return;

        GameEconomyManager.Instance.SpendCoins(coinCost);

        StopAllCoroutines();
        continuePanel.SetActive(false);

        Time.timeScale = 1f;

        GameManagerCycle.Instance.player.RestoreCheckpoint();
        GameManagerCycle.Instance.ResumeAfterContinue();
    }
}
