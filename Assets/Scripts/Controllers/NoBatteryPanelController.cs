using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NoBatteryPanelController : MonoBehaviour
{
    [Header("Buttons")]
    public Button buyBatteryButton;
    public Button watchAdButton;

    [Header("UI")]
    public TextMeshProUGUI timerText;

    [Header("Config")]
    public int batteryCost = 100;

    void OnEnable()
    {
        Time.timeScale = 1f;
        buyBatteryButton.onClick.RemoveAllListeners();
        watchAdButton.onClick.RemoveAllListeners();

        buyBatteryButton.onClick.AddListener(OnBuyBatteryClicked);
        watchAdButton.onClick.AddListener(OnWatchAdClicked);

        UpdateButtonStates();
    }

    void Update()
    {
        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        float seconds = BatteryManager.Instance.GetSecondsUntilNextBattery();

        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);

        timerText.text = $"{minutes:00}:{secs:00}";
    }

    // 🔥 BUY → RETURN TO PREVIOUS PANEL
    void OnBuyBatteryClicked()
    {
        if (GameEconomyManager.Instance.GetCoins() < batteryCost)
            return;

        GameEconomyManager.Instance.SpendCoins(batteryCost);
        BatteryManager.Instance.AddBatteryInstant(1);

        GameManagerCycle.Instance.ReturnFromNoBatteryPanel();
    }

    // 🔥 WATCH AD → RETURN TO PREVIOUS PANEL
    void OnWatchAdClicked()
    {
        BatteryManager.Instance.AddBatteryInstant(1);

        GameManagerCycle.Instance.ReturnFromNoBatteryPanel();
    }

    void UpdateButtonStates()
    {
        int coins = GameEconomyManager.Instance.GetCoins();

        buyBatteryButton.interactable = coins >= batteryCost;
        watchAdButton.interactable = true;
    }
}
