using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RevivePanelController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI timerText;
    public Button reviveButton;
    public Button skipButton;

    [Header("Settings")]
    public float reviveDuration = 10f;

    [Header("Cost")]
    public int reviveCost = 100;

    private float timer;
    private bool reviveUsed;

    void OnEnable()
    {
        timer = reviveDuration;
        reviveUsed = false;

        reviveButton.interactable = true;
        skipButton.interactable = true;

        if (GameManagerCycle.Instance != null && levelText != null)
        {
            levelText.text =
                "LEVEL " + GameManagerCycle.Instance.CurrentLevelNumber;
        }

        UpdateTimerUI();
        UpdateReviveButtonState();
    }

    void Update()
    {
        timer -= Time.unscaledDeltaTime;
        UpdateTimerUI();

        if (timer <= 0f)
            TimeUp();
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timer).ToString();
    }

    // ---------------- BUTTONS ----------------

    public void OnSpendCoinsClicked()
    {
        if (reviveUsed) return;

        if (!GameEconomyManager.Instance.SpendCoins(reviveCost))
            return;

        CompleteRevive();
    }

    public void OnWatchAdClicked()
    {
        if (reviveUsed) return;

        // Ad success assumed
        CompleteRevive();
    }

    void CompleteRevive()
    {
        reviveUsed = true;
        reviveButton.interactable = false;
        skipButton.interactable = false;

        GameManagerCycle.Instance.RevivePlayer();
        gameObject.SetActive(false);
    }

    // ---------------- INTERNAL ----------------

    void TimeUp()
    {
        if (reviveUsed) return;

        reviveUsed = true;
        GameManagerCycle.Instance.ShowGameOver();
        gameObject.SetActive(false);
    }

    // ---------------- API ----------------

    public void SetReviveCost(int cost)
    {
        reviveCost = cost;

        if (reviveButton != null)
        {
            TextMeshProUGUI txt =
                reviveButton.GetComponentInChildren<TextMeshProUGUI>();

            if (txt != null)
                txt.text = cost.ToString();
        }

        UpdateReviveButtonState();
    }

    void UpdateReviveButtonState()
    {
        reviveButton.interactable =
            GameEconomyManager.Instance.GetCoins() >= reviveCost;
    }
}
