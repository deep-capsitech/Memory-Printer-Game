using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NoBatteryPanelController : MonoBehaviour
{
    [Header("Buttons")]
    public Button closeButton;
    public Button buyBatteryButton;
    public Button watchAdButton;

    [Header("UI")]
    public TextMeshProUGUI timerText;

    [Header("Config")]
    public int batteryCost = 100;

    void Start()
    {
        closeButton.onClick.AddListener(OnCloseClicked);
        buyBatteryButton.onClick.AddListener(OnBuyBatteryClicked);
        watchAdButton.onClick.AddListener(OnWatchAdClicked);
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

    void OnBuyBatteryClicked()
    {
        if (GameEconomyManager.Instance.GetCoins() < batteryCost)
            return;

        GameEconomyManager.Instance.SpendCoins(batteryCost);
        BatteryManager.Instance.AddBatteryInstant(1);

        GameManagerCycle.Instance.ShowMenu();
    }

    void OnWatchAdClicked()
    {
        // TEMP: simulate rewarded ad success
        BatteryManager.Instance.AddBatteryInstant(1);

        GameManagerCycle.Instance.ShowMenu();
    }

    void OnCloseClicked()
    {
        GameManagerCycle.Instance.ShowMenu();
    }
}
