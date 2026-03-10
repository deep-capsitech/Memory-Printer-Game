using UnityEngine;
using System;

public class DailyRewardManager : MonoBehaviour
{
    public static DailyRewardManager Instance;

    // PlayerPrefs keys
    private const string DAY_KEY = "DailyRewardDay";
    private const string DATE_KEY = "DailyRewardLastClaimDate";

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

        // SAFETY FIX
        if (currentDay < 1 || currentDay > 7)
            currentDay = 1;

        Debug.Log("Daily Reward Day: " + currentDay);
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

    // ----------------------------
    // CLAIM REWARD
    // ----------------------------
    public void ClaimReward()
    {
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
        switch (day)
        {
            case 1:
                GameEconomyManager.Instance.AddCoins(50);
                break;

            case 2:
                BatteryManager.Instance.AddBatteryInstant(1);
                break;

            case 3:
                GameEconomyManager.Instance.AddCoins(75);
                break;

            case 4:
                // Freeze powerup (add later if needed)
                break;

            case 5:
                BatteryManager.Instance.AddBatteryInstant(2);
                break;

            case 6:
                GameEconomyManager.Instance.AddCoins(100);
                break;

            case 7:
                GameEconomyManager.Instance.AddCoins(200);
                break;
        }
    }

    public bool HasClaimedTodayPublic()
    {
        string lastDate = PlayerPrefs.GetString("DailyRewardLastClaimDate", "");
        string today = DateTime.UtcNow.ToString("yyyyMMdd");
        return lastDate == today;
    }

    // ----------------------------
    // GETTERS (FOR UI LATER)
    // ----------------------------
    public int GetCurrentDay()
    {
        return currentDay;
    }
}