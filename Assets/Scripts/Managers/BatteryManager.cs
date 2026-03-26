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
        currentBatteries = Mathf.Clamp(
     PlayerPrefs.GetInt(BATTERY_KEY, maxBatteries),
     0,
     maxBatteries
 );
        if (PlayerPrefs.HasKey(NEXT_TIME_KEY))
        {
            nextBatteryTime = DateTime.FromBinary(
    Convert.ToInt64(PlayerPrefs.GetString(NEXT_TIME_KEY))
);
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
            PlayerPrefs.SetString(NEXT_TIME_KEY, nextBatteryTime.ToBinary().ToString());
        else
            PlayerPrefs.DeleteKey(NEXT_TIME_KEY);

        PlayerPrefs.Save();
    }

    void RefillIfNeeded()
    {
        if (currentBatteries >= maxBatteries)
            return;

        if (nextBatteryTime == DateTime.MinValue)
        {
            nextBatteryTime = DateTime.UtcNow.AddMinutes(refillMinutes);
            SaveData();
            return;
        }

        DateTime now = DateTime.UtcNow;

        bool changed = false;

        while (currentBatteries < maxBatteries && now >= nextBatteryTime)
        {
            currentBatteries++;
            nextBatteryTime = nextBatteryTime.AddMinutes(refillMinutes);
            changed = true;
        }

        if (currentBatteries >= maxBatteries)
        {
            nextBatteryTime = DateTime.MinValue;
            changed = true;
        }

        if (changed)
            SaveData();
    }

    public bool HasBattery()
    {
        RefillIfNeeded();
        return currentBatteries > 0;
    }

    public void ConsumeBattery()
    {

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

        TimeSpan remaining = nextBatteryTime - DateTime.UtcNow;
        return Mathf.Max(0f, (float)remaining.TotalSeconds);
    }

    public void AddBatteryInstant(int amount = 1)
    {
        RefillIfNeeded();

        currentBatteries = Mathf.Min(maxBatteries, currentBatteries + amount);

        // If now full, clear timer
        if (currentBatteries >= maxBatteries)
        {
            nextBatteryTime = DateTime.MinValue;
        }

        SaveData();
    }

}
