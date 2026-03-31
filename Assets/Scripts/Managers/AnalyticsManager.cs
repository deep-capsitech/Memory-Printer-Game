using UnityEngine;
using Firebase.Analytics;
using System;
using System.Collections.Generic;

public static class AnalyticsManager
{
    /// <summary>
    /// Core safe event logger used by all analytics
    /// </summary>
    public static void LogEvent(string eventName, params (string key, object value)[] parameters)
    {
        if (!FirebaseManager.IsFirebaseReady)
        {
            Debug.Log("Firebase not ready. Event skipped: " + eventName);
            return;
        }

        try
        {
            List<Parameter> firebaseParams = new List<Parameter>();

            foreach (var p in parameters)
            {
                if (p.value is int)
                    firebaseParams.Add(new Parameter(p.key, (int)p.value));

                else if (p.value is float)
                    firebaseParams.Add(new Parameter(p.key, (float)p.value));

                else if (p.value is double)
                    firebaseParams.Add(new Parameter(p.key, (double)p.value));

                else if (p.value is string)
                    firebaseParams.Add(new Parameter(p.key, (string)p.value));

                else
                    firebaseParams.Add(new Parameter(p.key, p.value.ToString()));
            }

            if (firebaseParams.Count > 0)
                FirebaseAnalytics.LogEvent(eventName, firebaseParams.ToArray());
            else
                FirebaseAnalytics.LogEvent(eventName);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Analytics failed: " + e.Message);
        }
    }

    // =========================================================
    // GAME EVENTS
    // =========================================================

    public static void LogGameStart()
    {
        LogEvent("game_start");
    }

    public static void LogGameOver(int levelIndex)
    {
        LogEvent("game_over",
            ("score", levelIndex));
    }

    // =========================================================
    // LEVEL EVENTS
    // =========================================================

    public static void LogLevelStart(int levelIndex)
    {
        currentLevel = levelIndex;
        levelStartTime = Time.time;

        LogEvent("level_start",
            ("level_index", levelIndex));
    }

    public static void LogLevelRetry(int levelIndex)
    {
        if (!levelRetryCounts.ContainsKey(levelIndex))
            levelRetryCounts[levelIndex] = 0;

        levelRetryCounts[levelIndex]++;

        LogEvent("level_retry",
            ("level_index", levelIndex),
            ("retry_count", levelRetryCounts[levelIndex]));
    }

    public static void LogLevelComplete(int levelIndex, int score, int stars)
    {
        float timeAlive = Time.time - levelStartTime;

        LogEvent("level_complete",
            ("level_index", levelIndex),
            ("level_score", score),
            ("stars", stars),
            ("time_alive", timeAlive));

        ResetLevelStats(levelIndex);
    }

    // =========================================================
    // LEVEL FAIL + DIFFICULTY SPIKE DETECTION
    // =========================================================

    static Dictionary<int, int> levelFailCounts = new Dictionary<int, int>();
    static int difficultySpikeThreshold = 5;

    static float levelStartTime;
    static int currentLevel;

    public static void RegisterLevelFail(int levelIndex)
    {
        float timeAlive = Time.time - levelStartTime;

        if (!levelFailCounts.ContainsKey(levelIndex))
            levelFailCounts[levelIndex] = 0;

        levelFailCounts[levelIndex]++;

        int failCount = levelFailCounts[levelIndex];

        LogEvent("level_fail",
            ("level_index", levelIndex),
            ("fail_count", failCount),
            ("time_alive", timeAlive));

        // Difficulty spike detection
        if (failCount >= difficultySpikeThreshold)
        {
            LogEvent("difficulty_spike",
                ("level_index", levelIndex),
                ("fail_count", failCount),
                ("time_alive", timeAlive));

            levelFailCounts[levelIndex] = 0;
        }
    }

    // =========================================================
    // RAGE QUIT DETECTION
    // =========================================================

    static Dictionary<int, int> levelRetryCounts = new Dictionary<int, int>();
    static int rageQuitThreshold = 6;

    public static void CheckRageQuit(int levelIndex)
    {
        if (!levelRetryCounts.ContainsKey(levelIndex))
            return;

        int retryCount = levelRetryCounts[levelIndex];

        if (retryCount >= rageQuitThreshold)
        {
            LogEvent("rage_quit",
                ("level_index", levelIndex),
                ("retry_count", retryCount));
        }
    }

    // =========================================================
    // ECONOMY EVENTS
    // =========================================================

    public static void LogCoinCollected(int totalCoins)
    {
        LogEvent("coin_collected",
            ("total_coins", totalCoins));
    }

    public static void LogStarsEarned(int stars)
    {
        LogEvent("stars_earned",
            ("star_count", stars));
    }

    // =========================================================
    // POWERUPS
    // =========================================================

    public static void LogPowerUpUsed(string powerupName)
    {
        LogEvent("powerup_used",
            ("powerup_name", powerupName),
            ("level_index", currentLevel));
    }

    // =========================================================
    // RESET LEVEL DATA
    // =========================================================

    static void ResetLevelStats(int levelIndex)
    {
        if (levelFailCounts.ContainsKey(levelIndex))
            levelFailCounts[levelIndex] = 0;

        if (levelRetryCounts.ContainsKey(levelIndex))
            levelRetryCounts[levelIndex] = 0;
    }
}