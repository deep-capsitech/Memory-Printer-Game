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

    private DailyRewardData[] rewardTable = new DailyRewardData[]
{
    new DailyRewardData { rewardName = "COINS",     amount = 50  },
    new DailyRewardData { rewardName = "BATTERY",   amount = 1   },
    new DailyRewardData { rewardName = "COINS",     amount = 75  },
    new DailyRewardData { rewardName = "FREEZE",    amount = 1   },
    new DailyRewardData { rewardName = "BATTERIES", amount = 2   },
    new DailyRewardData { rewardName = "COINS",     amount = 100 },
    new DailyRewardData { rewardName = "COINS",     amount = 200 }
};
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

    // ----------------------------
    // DATE CHECK
    // ----------------------------
    bool HasClaimedToday()
    {
        string lastDate = PlayerPrefs.GetString(DATE_KEY, "");
        string today = DateTime.UtcNow.ToString("yyyyMMdd");
        return lastDate == today;
    }

    public bool CanShowDailyReward()
    {
        return !HasClaimedToday();
    }

    public bool HasClaimedTodayPublic()
    {
        string lastDate = PlayerPrefs.GetString(DATE_KEY, "");
        string today = DateTime.UtcNow.ToString("yyyyMMdd");
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
        string today = DateTime.UtcNow.ToString("yyyyMMdd");

        if (lastPopupDate == today)
            return false;

        PlayerPrefs.SetString(DAILY_POPUP_DATE, today);
        PlayerPrefs.Save();

        return true;
    }

    // ----------------------------
    // CLAIM REWARD
    // ----------------------------
    //public void ClaimReward()
    //{
    //    GiveRewardForDay(currentDay);

    //    string today = DateTime.UtcNow.ToString("yyyyMMdd");
    //    PlayerPrefs.SetString(DATE_KEY, today);

    //    currentDay++;
    //    if (currentDay > 7)
    //        currentDay = 1;

    //    PlayerPrefs.SetInt(DAY_KEY, currentDay);
    //    PlayerPrefs.Save();

    //    UpdateDailyRewardButton();
    //}
    public void ClaimReward()
    {
        if (!CanShowDailyReward())
            return;

        GiveRewardForDay(currentDay);

        string today = DateTime.UtcNow.ToString("yyyyMMdd");
        PlayerPrefs.SetString(DATE_KEY, today);

        currentDay++;

        if (currentDay > 7)
            currentDay = 1;

        PlayerPrefs.SetInt(DAY_KEY, currentDay);
        PlayerPrefs.Save();
    }
    // ----------------------------
    // REWARD TABLE
    // ----------------------------
    void GiveRewardForDay(int day)
    {
        int index = day - 1;

        if (index < 0 || index >= rewardTable.Length)
            return;

        var reward = rewardTable[index];

        switch (reward.rewardName)
        {
            case "COINS":
                GameEconomyManager.Instance.AddCoins(reward.amount);
                break;

            case "BATTERY":
            case "BATTERIES":
                BatteryManager.Instance.AddBatteryInstant(reward.amount);
                break;

            case "FREEZE":
                // future powerup reward
                break;
        }
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

    public int GetRewardAmount(int day)
    {
        int index = day - 1;

        if (index < 0 || index >= rewardTable.Length)
            return 0;

        return rewardTable[index].amount;
    }

    public string GetRewardName(int day)
    {
        int index = day - 1;

        if (index < 0 || index >= rewardTable.Length)
            return "";

        return rewardTable[index].rewardName;
    }

    int GetDaysDifference(string lastDateString)
    {
        if (string.IsNullOrEmpty(lastDateString))
            return 0;

        DateTime lastDate = DateTime.ParseExact(lastDateString, "yyyyMMdd", null);
        DateTime today = DateTime.UtcNow.Date;

        return (today - lastDate).Days;
    }
}