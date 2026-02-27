using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProgressionController : MonoBehaviour
{
    [Header("Config")]
    public int totalLevels = 50;

    [Header("Star UI")]
    public Image star1;
    public Image star2;
    public Image star3;
    public Sprite filledStar;
    public Sprite emptyStar;

    //[Header("New World Panel")]
    //public GameObject newWorldPanel;
    //public TextMeshProUGUI newWorldNameText;
    //public TextMeshProUGUI newWorldQuestionText;

    [Header("Dependencies")]
    public UIFlowController uiFlowController;

    private int _earnedStars;
    private int _totalStars;
    private int _highestLevel;
    //private WorldData _pendingUnlockedWorld;

    // ---------- STARS ----------

    public void CalculateStars(float levelTimer)
    {
        float maxLevelTime = JsonLevelLoader.Instance
            .GetLevel(GameManagerCycle.Instance.CurrentLevelNumber)
            .levelTime;

        float timeTaken = maxLevelTime - levelTimer;

        if (timeTaken <= 20f)
            _earnedStars = 3;
        else if (timeTaken <= 40f)
            _earnedStars = 2;
        else
            _earnedStars = 1;

        SaveLevelStars();
        ShowStars(_earnedStars);
    }

    void SaveLevelStars()
    {
        int level = GameManagerCycle.Instance.CurrentLevelNumber;

        string key = "LevelStars" + level;
        int previous = PlayerPrefs.GetInt(key, 0);

        if (_earnedStars > previous)
        {
            int diff = _earnedStars - previous;
            _totalStars += diff;

            PlayerPrefs.SetInt(key, _earnedStars);
            PlayerPrefs.SetInt("TotalStar", _totalStars);
            PlayerPrefs.Save();
        }
    }

    void ShowStars(int count)
    {
        star1.sprite = count >= 1 ? filledStar : emptyStar;
        star2.sprite = count >= 2 ? filledStar : emptyStar;
        star3.sprite = count >= 3 ? filledStar : emptyStar;
    }

    // ---------- COINS ----------

    public void GiveCoinsForStars()
    {
        int coins = _earnedStars switch
        {
            3 => 15,
            2 => 10,
            1 => 5,
            _ => 0
        };

        GameEconomyManager.Instance.AddCoins(coins);
    }

    // ---------- LEVELS ----------

    public void LoadHighestLevel()
    {
        _highestLevel = PlayerPrefs.GetInt("HighestLevel", 1);
    }

    public void UpdateHighestLevel(int currentLevel)
    {
        if (currentLevel > _highestLevel)
        {
            _highestLevel = currentLevel;
            PlayerPrefs.SetInt("HighestLevel", _highestLevel);
            PlayerPrefs.Save();
        }
    }

    public void ClearLevelFailed(int level)
    {
        PlayerPrefs.DeleteKey($"LevelFailed_{level}");
    }

    // ---------- WORLDS ----------

    bool IsWorldUnlocked(int worldId)
    {
        return PlayerPrefs.GetInt($"WorldUnlocked_{worldId}", 0) == 1;
    }

    void UnlockWorld(int worldId)
    {
        PlayerPrefs.SetInt($"WorldUnlocked_{worldId}", 1);
        PlayerPrefs.Save();
    }

    bool IsWorldUnlockPopupShown(int worldId)
    {
        return PlayerPrefs.GetInt($"WorldUnlockPopupShown_{worldId}", 0) == 1;
    }

    void MarkWorldUnlockPopupShown(int worldId)
    {
        PlayerPrefs.SetInt($"WorldUnlockPopupShown_{worldId}", 1);
        PlayerPrefs.Save();
    }

    public void CheckForNewWorldUnlock()
    {
        _totalStars = PlayerPrefs.GetInt("TotalStar", 0);

        foreach (WorldData world in WorldDatabase.Instance.GetWorlds())
        {
            if (world.worldId == 1) continue;
            if (IsWorldUnlocked(world.worldId)) continue;
            if (_totalStars < world.starsRequired) continue;

            UnlockWorld(world.worldId);

            int firstLevel = (world.worldId - 1) * 10 + 1;
            int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);

            if (unlocked < firstLevel)
            {
                PlayerPrefs.SetInt("UnlockedLevel", firstLevel);
                PlayerPrefs.Save();
            }

            if (!IsWorldUnlockPopupShown(world.worldId))
            {
                MarkWorldUnlockPopupShown(world.worldId);
                GameManagerCycle.Instance.ShowNewWorldUnlockedPanel(world);
            }

            break;
        }
    }
    public void UnlockNextLevel(int completedLevel, int totalLevels)
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        // Unlock NEXT level only if player just completed the highest unlocked one
        if (completedLevel == unlockedLevel && completedLevel < totalLevels)
        {
            PlayerPrefs.SetInt("UnlockedLevel", unlockedLevel + 1);
            PlayerPrefs.Save();
        }
    }
}