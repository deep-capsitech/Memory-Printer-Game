using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[System.Serializable]
public struct DailyRewardData
{
    public string rewardName;
    public int amount;
}
public class DailyRewardController : MonoBehaviour
{
    public static DailyRewardController Instance;

    [Header("Daily Reward Panel")]
    public GameObject dailyRewardPanel;

    [Header("Daily Reward Button")]
    public Button dailyRewardButton;
    public TextMeshProUGUI dailyRewardButtonText;
    public Image dailyRewardButtonIcon;

    // PlayerPrefs keys
    private const string DAY_KEY = "DailyRewardDay";
    private const string DATE_KEY = "DailyRewardLastClaimDate";
    private const string DAILY_POPUP_DATE = "DailyRewardPopupDate";

    private int currentDay; // 1–7

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadState();
    }
    void LoadState()
    {
        currentDay = PlayerPrefs.GetInt(DAY_KEY, 1);

        if (currentDay < 1 || currentDay > 7)
            currentDay = 1;

        string lastDate = PlayerPrefs.GetString(DATE_KEY, "");

        if (!string.IsNullOrEmpty(lastDate))
        {
            int diff = GetDaysDifference(lastDate);

            if (diff > 1)
            {
                // 🔥 Missed day → reset streak
                currentDay = 1;
                PlayerPrefs.SetInt(DAY_KEY, currentDay);
                PlayerPrefs.Save();
            }
        }
    }
    // ----------------------------
    // INITIALIZE UI
    // ----------------------------
    public void Initialize()
    {
        UpdateDailyRewardButton();
    }
    bool HasClaimedToday()
    {
        string lastDate = PlayerPrefs.GetString(DATE_KEY, "");
        string today = DateTime.Now.ToString("yyyyMMdd");
        return lastDate == today;
    }

    public bool CanShowDailyReward()
    {
        return !HasClaimedToday();
    }
    public bool HasClaimedTodayPublic()
    {
        string lastDate = PlayerPrefs.GetString(DATE_KEY, "");
        string today = DateTime.Now.ToString("yyyyMMdd");
        return lastDate == today;
    }
    // ----------------------------
    // AUTO POPUP LOGIC
    // ----------------------------
    public bool ShouldAutoShowDailyReward()
    {
        if (!CanShowDailyReward())
            return false;

        string lastPopupDate = PlayerPrefs.GetString(DAILY_POPUP_DATE, "");
        string today = DateTime.Now.ToString("yyyyMMdd");

        if (lastPopupDate == today)
            return false;

        PlayerPrefs.SetString(DAILY_POPUP_DATE, today);
        PlayerPrefs.Save();

        return true;
    }
    public void ClaimReward()
    {
        if (!CanShowDailyReward())
            return;

        DailyReward reward = GetRewardForDay(currentDay);

        GiveReward(reward);

        // Day 7 Bonus
        if (currentDay == 7)
        {
            GiveDaySevenBonus();
        }

        string today = DateTime.Now.ToString("yyyyMMdd");
        PlayerPrefs.SetString(DATE_KEY, today);

        currentDay++;
        if (currentDay > 7)
            currentDay = 1;

        PlayerPrefs.SetInt(DAY_KEY, currentDay);
        PlayerPrefs.Save();
    }

    // ----------------------------
    // UI UPDATE
    // ----------------------------
    public void UpdateDailyRewardButton()
    {
        bool canClaim = CanShowDailyReward();

        dailyRewardButton.interactable = true;

        if (canClaim)
        {
            dailyRewardButtonText.text = "CLAIM";
            dailyRewardButtonIcon.color = Color.white;
        }
        else
        {
            dailyRewardButtonText.text = "CLAIMED";
            dailyRewardButtonIcon.color = Color.white;
        }
    }

    public void OnDailyRewardButtonClicked(UIFlowController uiFlowController)
    {
        uiFlowController.ShowDailyRewardPanel();
    }

    // ----------------------------
    // GETTERS
    // ----------------------------
    public int GetCurrentDay()
    {
        return currentDay;
    }
    int GetDaysDifference(string lastDateString)
    {
        if (string.IsNullOrEmpty(lastDateString))
            return 0;

        DateTime lastDate = DateTime.ParseExact(lastDateString, "yyyyMMdd", null);
        DateTime today = DateTime.Now.Date;

        return (today - lastDate).Days;
    }
    public enum DailyRewardType
    {
        Snapshot,
        Invision,
        Freeze,
        Coins
    }

    public struct DailyReward
    {
        public DailyRewardType type;
        public int amount;

        public DailyReward(DailyRewardType t, int a)
        {
            type = t;
            amount = a;
        }
    }
    public DailyReward GetRewardForDay(int day)
    {
        int world = PlayerPrefs.GetInt("SelectedWorld", 1);

        switch (day)
        {
            case 1:
                return new DailyReward(DailyRewardType.Snapshot, 1);

            case 2:
                return new DailyReward(DailyRewardType.Coins, 50);

            case 3:
                return new DailyReward(DailyRewardType.Snapshot, 1);

            case 4:
                if (world >= 2)
                    return new DailyReward(DailyRewardType.Invision, 1);
                else
                    return new DailyReward(DailyRewardType.Snapshot, 1);

            case 5:
                return new DailyReward(DailyRewardType.Coins, 100);

            case 6:
                return new DailyReward(DailyRewardType.Snapshot, 1);

            case 7:
                if (world >= 3)
                    return new DailyReward(DailyRewardType.Freeze, 1);
                else if (world >= 2)
                    return new DailyReward(DailyRewardType.Invision, 1);
                else
                    return new DailyReward(DailyRewardType.Snapshot, 2);

            default:
                return new DailyReward(DailyRewardType.Coins, 50);
        }
    }
    void GiveReward(DailyReward reward)
    {
        switch (reward.type)
        {
            case DailyRewardType.Snapshot:
                for (int i = 0; i < reward.amount; i++)
                    GameManagerCycle.Instance.AddSnapshotUse();
                break;

            case DailyRewardType.Invision:
                PowerupInventoryManager.Instance.AddInvision(reward.amount);
                break;

            case DailyRewardType.Freeze:
                PowerupInventoryManager.Instance.AddFreeze(reward.amount);
                break;

            case DailyRewardType.Coins:
                GameEconomyManager.Instance.AddCoins(reward.amount);
                break;
        }

        GameManagerCycle.Instance.powerUpController.UpdatePowerUpUI();
    }
    void GiveDaySevenBonus()
    {
        int bonusCoins = 200;
        GameEconomyManager.Instance.AddCoins(bonusCoins);
    }
    public void GiveExtraReward(DailyReward reward)
    {
        GiveReward(reward);
    }
}