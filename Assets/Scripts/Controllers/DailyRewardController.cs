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

    // PlayerPrefs keys
    private const string DAY_KEY = "DailyRewardDay";
    private const string DATE_KEY = "DailyRewardLastClaimDate";
    private const string DAILY_POPUP_DATE = "DailyRewardPopupDate";

    private int currentDay;
    private string today;
    private string lastClaimDate;

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
        today = DateTime.UtcNow.ToString("yyyyMMdd");
        currentDay = PlayerPrefs.GetInt(DAY_KEY, 1);

        if (currentDay < 1 || currentDay > 7)
            currentDay = 1;

        lastClaimDate = PlayerPrefs.GetString(DATE_KEY, "");

        if (!string.IsNullOrEmpty(lastClaimDate))
        {
            int diff = GetDaysDifference(lastClaimDate);

            if (diff > 1)
            {
                currentDay = 1;
                PlayerPrefs.SetInt(DAY_KEY, currentDay);
                PlayerPrefs.Save();
            }
        }
    }

    public void Initialize()
    {
        UpdateDailyRewardButton();
    }
    public bool HasClaimedTodayPublic()
    {
        return lastClaimDate == today;
    }

    public bool CanShowDailyReward()
    {
        return !HasClaimedTodayPublic();
    }

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

        string today = this.today;
        PlayerPrefs.SetString(DATE_KEY, today);
        lastClaimDate = today;

        currentDay++;
        if (currentDay > 7)
            currentDay = 1;

        PlayerPrefs.SetInt(DAY_KEY, currentDay);
        PlayerPrefs.Save();
    }

    public void UpdateDailyRewardButton()
    {
        dailyRewardButton.interactable = true;
    }

    public void OnDailyRewardButtonClicked(UIFlowController uiFlowController)
    {
        uiFlowController.ShowDailyRewardPanel();
    }

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
        switch (day)
        {
            case 1:
                return new DailyReward(DailyRewardType.Coins, 100);

            case 2:
                return new DailyReward(DailyRewardType.Snapshot, 1);

            case 3:
                return new DailyReward(DailyRewardType.Invision, 1);

            case 4:
                return new DailyReward(DailyRewardType.Coins, 150);

            case 5:
                return new DailyReward(DailyRewardType.Invision, 1);

            case 6:
                return new DailyReward(DailyRewardType.Freeze, 1);

            case 7:
                return new DailyReward(DailyRewardType.Coins, 0);
        }

        return new DailyReward(DailyRewardType.Coins, 50);
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
        GameEconomyManager.Instance.AddCoins(200);
        PowerupInventoryManager.Instance.AddInvision(1);
        PowerupInventoryManager.Instance.AddFreeze(1);
    }
    public void GiveExtraReward(DailyReward reward)
    {
        GiveReward(reward);
    }
}