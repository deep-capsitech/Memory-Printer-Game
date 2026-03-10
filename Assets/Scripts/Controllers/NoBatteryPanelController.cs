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
        if (BatteryManager.Instance.HasBattery())
        {
            ClosePanel();
        }
    }
    void Update()
    {
        UpdateTimerUI();

        // Safety check → if battery available close panel
        if (BatteryManager.Instance.HasBattery())
        {
            ClosePanel();
            return;
        }

        // Safety check → timer reached zero
        if (BatteryManager.Instance.GetSecondsUntilNextBattery() <= 0f)
        {
            ClosePanel();
        }
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

        GameManagerCycle.Instance.uiFlowController.ReturnFromNoBatteryPanel();
    }

    void OnWatchAdClicked()
    {
        AdManager.Instance.ShowRewarded(() =>
        {
            BatteryManager.Instance.AddBatteryInstant(1);

            GameManagerCycle.Instance.uiFlowController.ReturnFromNoBatteryPanel();
        });
    }

    void UpdateButtonStates()
    {
        int coins = GameEconomyManager.Instance.GetCoins();

        buyBatteryButton.interactable = coins >= batteryCost;
        watchAdButton.interactable = true;
    }

    void ClosePanel()
    {
        gameObject.SetActive(false);
        GameManagerCycle.Instance.uiFlowController.ReturnFromNoBatteryPanel();
    }
}
