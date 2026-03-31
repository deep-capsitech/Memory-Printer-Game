using UnityEngine;

public class GameEconomyManager : MonoBehaviour
{
    public static GameEconomyManager Instance;

    private const string COINS_KEY = "TOTAL_COINS";
    private const string FIRST_LAUNCH_KEY = "FIRST_LAUNCH_DONE";

    private int totalCoins;
    private int levelCoinsEarned;


    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeCoins();
    }

    void InitializeCoins()
    {
        // First install logic
        if (!PlayerPrefs.HasKey(FIRST_LAUNCH_KEY))
        {
            totalCoins = 0;
            PlayerPrefs.SetInt(COINS_KEY, totalCoins);
            PlayerPrefs.SetInt(FIRST_LAUNCH_KEY, 1);
            PlayerPrefs.Save();
        }
        else
        {
            totalCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
        }
    }

    public int GetCoins()
    {
        return totalCoins;
    }

    public void AddCoins(int amount)
    {
        levelCoinsEarned += amount;
        totalCoins += amount;
        PlayerPrefs.SetInt(COINS_KEY, totalCoins);
        PlayerPrefs.Save();
        AnalyticsManager.LogEvent("coins_earned",
      ("amount", amount),
      ("total_coins", totalCoins),
      ("level_coins", levelCoinsEarned));
    }

    public bool SpendCoins(int amount)
    {
        if (totalCoins < amount)
            return false;

        totalCoins -= amount;
        PlayerPrefs.SetInt("TOTAL_COINS", totalCoins);
        PlayerPrefs.Save();
        AnalyticsManager.LogEvent("coins_spent",
        ("amount", amount),
        ("total_coins", totalCoins));

        return true;
    }
    public void ResetLevelCoins()
    {
        levelCoinsEarned = 0;
        AnalyticsManager.LogEvent("level_coin_reset");
    }

    public int GetLevelCoins()
    {
        return levelCoinsEarned;
    }

}
