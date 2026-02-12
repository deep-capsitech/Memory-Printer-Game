using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RevivePanelController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelText;   
    public TextMeshProUGUI timerText;     // TimerTxt
    public Button reviveButton;           // SpendCoins
    public Button skipButton;             // WatchAd

    [Header("Settings")]
    public float reviveDuration = 10f;

    private float timer;
    private bool reviveUsed;

    [Header("Cost")]
    public int reviveCost = 100;


    void OnEnable()
    {
        timer = reviveDuration;
        reviveUsed = false;

        reviveButton.interactable = true;
        skipButton.interactable = true;

        // ✅ READ level number from GameManager
        if (GameManagerCycle.Instance != null && levelText != null)
        {
            levelText.text = "LEVEL " + GameManagerCycle.Instance.CurrentLevelNumber;
        }

        UpdateTimerUI();
    }


    void Update()
    {
        // Game is paused → use unscaled time
        timer -= Time.unscaledDeltaTime;

        UpdateTimerUI();

        if (timer <= 0f)
        {
            TimeUp();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timer).ToString();
    }

    // ================= BUTTON CALLBACKS =================

    // Spend Coins button
    public void OnSpendCoinsClicked()
    {
        if (reviveUsed) return;

        // ❌ Not enough coins → do nothing
        if (!GameEconomyManager.Instance.SpendCoins(reviveCost))
        {
            Debug.Log("Not enough coins to revive");
            return;
        }

        reviveUsed = true;
        reviveButton.interactable = false;
        skipButton.interactable = false;

        GameManagerCycle.Instance.RevivePlayer();
        gameObject.SetActive(false);
    }

    // Watch Ad button (acts as Skip for now)
    public void OnWatchAdClicked()
    {
        if (reviveUsed) return;

        reviveUsed = true;
        reviveButton.interactable = false;
        skipButton.interactable = false;

        // Ad logic will be added later
        GameManagerCycle.Instance.RevivePlayer();
        gameObject.SetActive(false);
    }

    // ================= INTERNAL =================

    void TimeUp()
    {
        if (reviveUsed) return;

        reviveUsed = true;
        GameManagerCycle.Instance.ShowGameOver();
        gameObject.SetActive(false);
    }


}
