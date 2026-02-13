using UnityEngine;

public class LevelPanelController : MonoBehaviour
{
    [Header("References")]
    public Transform contentParent;
    public GameObject levelButtonPrefab;

    [Header("Config")]
    public int totalLevels = 15;

    void OnEnable()
    {
        GenerateLevels();
    }

    //void GenerateLevels()
    //{
    //    ClearOldButtons();

    //    int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

    //    for (int i = 1; i <= totalLevels; i++)
    //    {
    //        GameObject btnObj = Instantiate(levelButtonPrefab, contentParent);

    //        LevelSelector selector = btnObj.GetComponent<LevelSelector>();
    //        if (selector == null)
    //        {
    //            Debug.LogError("LevelSelector missing on LevelButton prefab");
    //            continue;
    //        }

    //        bool unlocked = i <= unlockedLevel;
    //        int stars = PlayerPrefs.GetInt("LevelStars" + i, 0);

    //        selector.Setup(i, unlocked, stars);
    //    }
    //}
    void GenerateLevels()
    {
        ClearOldButtons();

        int worldIndex = PlayerPrefs.GetInt("SelectedWorld", 0);
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        int levelsPerWorld = 10;
        int startLevel = worldIndex * levelsPerWorld + 1;
        int endLevel = startLevel + levelsPerWorld - 1;

        for (int level = startLevel; level <= endLevel; level++)
        {
            GameObject btnObj = Instantiate(levelButtonPrefab, contentParent);
            LevelSelector selector = btnObj.GetComponent<LevelSelector>();

            bool unlocked = level <= unlockedLevel;
            int stars = PlayerPrefs.GetInt("LevelStars" + level, 0);

            selector.Setup(level, unlocked, stars);
        }
    }

    void ClearOldButtons()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);
    }

    public void OnBackClicked()
    {
        GameManagerCycle.Instance.BackToWorldPanel();
    }
}
