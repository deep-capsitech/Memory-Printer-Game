using UnityEngine;

using UnityEngine.UI;

using System.Collections;

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

    [Header("Dependencies")]

    public UIFlowController uiFlowController;

    private int _earnedStars;

    private int _totalStars;

    public void CalculateStars(float levelTimer)

    {

        int level = GameManagerCycle.Instance.CurrentLevelNumber;

        // Tutorial always gives 3 stars

        if (level == 1)

        {

            _earnedStars = 3;
            AnalyticsManager.LogStarsEarned(_earnedStars);

            SaveLevelStars();

            ShowStars(_earnedStars);

            return;

        }

        float maxLevelTime = JsonLevelLoader.Instance

            .GetLevel(level)

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

        _totalStars = PlayerPrefs.GetInt("TotalStar", 0);

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

        StartCoroutine(PlayStarAnimation(count));

    }

    IEnumerator PlayStarAnimation(int count)

    {

        // Reset all stars first

        star1.sprite = emptyStar;

        star2.sprite = emptyStar;

        star3.sprite = emptyStar;

        star1.transform.localScale = Vector3.zero;

        star2.transform.localScale = Vector3.zero;

        star3.transform.localScale = Vector3.zero;

        yield return new WaitForSeconds(0.15f);

        if (count >= 1)

        {

            yield return AnimateStar(star1);

        }

        if (count >= 2)

        {

            yield return new WaitForSeconds(0.1f);

            yield return AnimateStar(star2);

        }

        if (count >= 3)

        {

            yield return new WaitForSeconds(0.1f);

            yield return AnimateStar(star3);

        }

    }

    IEnumerator AnimateStar(Image star)

    {

        star.sprite = filledStar;

        float duration = 0.15f;

        float t = 0;

        Vector3 start = Vector3.zero;

        Vector3 overshoot = Vector3.one * 1.3f;

        Vector3 end = Vector3.one;

        // Scale up (pop)

        while (t < duration)

        {

            t += Time.deltaTime;

            star.transform.localScale = Vector3.Lerp(start, overshoot, t / duration);

            yield return null;

        }

        t = 0;

        // Settle back

        while (t < duration)

        {

            t += Time.deltaTime;

            star.transform.localScale = Vector3.Lerp(overshoot, end, t / duration);

            yield return null;

        }

        star.transform.localScale = end;

    }

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

    public void ClearLevelFailed(int level)

    {

        PlayerPrefs.DeleteKey($"LevelFailed_{level}");

    }

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
            AnalyticsManager.LogEvent("world_unlocked",
    ("world_id", world.worldId),
    ("stars_required", world.starsRequired),
    ("total_stars", _totalStars),
    ("level_reached", GameManagerCycle.Instance.CurrentLevelNumber));

            int firstLevel = (world.worldId - 1) * 10 + 1;

            int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);

            if (unlocked < firstLevel)

            {

                PlayerPrefs.SetInt("UnlockedLevel", firstLevel);

                PlayerPrefs.Save();

            }

            if (!IsWorldUnlockPopupShown(world.worldId))

            {
                AnalyticsManager.LogEvent("world_unlock_popup_shown",
    ("world_id", world.worldId));

                MarkWorldUnlockPopupShown(world.worldId);

                GameManagerCycle.Instance.ShowNewWorldUnlockedPanel(world);

            }

            break;

        }

    }

    public void UnlockNextLevel(int completedLevel, int totalLevels)

    {

        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        if (completedLevel == unlockedLevel && completedLevel < totalLevels)

        {

            PlayerPrefs.SetInt("UnlockedLevel", unlockedLevel + 1);

            PlayerPrefs.Save();

        }

    }
    public int GetEarnedStars()
    {
        return _earnedStars;
    }

}
