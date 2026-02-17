using UnityEngine;
using System;

public class BatteryManager : MonoBehaviour
{
    public static BatteryManager Instance;

    private const string BATTERY_KEY = "BATTERY_COUNT";
    private const string NEXT_TIME_KEY = "NEXT_BATTERY_TIME";

    public int maxBatteries = 5;
    public int refillMinutes = 30;

    private int currentBatteries;
    private DateTime nextBatteryTime;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadData();
        RefillIfNeeded();
    }

    void LoadData()
    {
        currentBatteries = PlayerPrefs.GetInt(BATTERY_KEY, maxBatteries);

        if (PlayerPrefs.HasKey(NEXT_TIME_KEY))
        {
            nextBatteryTime = DateTime.Parse(PlayerPrefs.GetString(NEXT_TIME_KEY));
        }
        else
        {
            nextBatteryTime = DateTime.MinValue;
        }
    }

    void SaveData()
    {
        PlayerPrefs.SetInt(BATTERY_KEY, currentBatteries);

        if (currentBatteries < maxBatteries)
            PlayerPrefs.SetString(NEXT_TIME_KEY, nextBatteryTime.ToString());
        else
            PlayerPrefs.DeleteKey(NEXT_TIME_KEY);

        PlayerPrefs.Save();
    }

    void RefillIfNeeded()
    {
        if (currentBatteries >= maxBatteries)
            return;

        DateTime now = DateTime.UtcNow;

        while (currentBatteries < maxBatteries && now >= nextBatteryTime)
        {
            currentBatteries++;
            nextBatteryTime = nextBatteryTime.AddMinutes(refillMinutes);
        }

        if (currentBatteries >= maxBatteries)
            nextBatteryTime = DateTime.MinValue;

        SaveData();
    }

    public bool HasBattery()
    {
        RefillIfNeeded();
        return currentBatteries > 0;
    }

    public void ConsumeBattery()
    {
        RefillIfNeeded();

        if (currentBatteries <= 0)
            return;

        bool wasFull = currentBatteries == maxBatteries;

        currentBatteries--;

        if (wasFull)
        {
            nextBatteryTime = DateTime.UtcNow.AddMinutes(refillMinutes);
        }

        SaveData();
    }

    public int GetBatteryCount()
    {
        RefillIfNeeded();
        return currentBatteries;
    }

    public float GetSecondsUntilNextBattery()
    {
        if (currentBatteries >= maxBatteries)
            return 0f;

        return Mathf.Max(
            0f,
            (float)(nextBatteryTime - DateTime.UtcNow).TotalSeconds
        );
    }
}
