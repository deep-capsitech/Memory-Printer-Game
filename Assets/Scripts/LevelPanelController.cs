using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPanelController : MonoBehaviour
{
    [Header("References")]
    public Transform contentParent;
    public GameObject levelButtonPrefab;

    [Header("Config")]
    public int totalLevels = 15;

    [Header("Theme UI")]
    public TextMeshProUGUI worldNameText;
    public Image panelFrame;

    void OnEnable()
    {
        GenerateLevels();
    }

    //void GenerateLevels()
    //{
    //    ClearOldButtons();

    //    int worldIndex = PlayerPrefs.GetInt("SelectedWorld", 0);
    //    int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

    //    int levelsPerWorld = 10;
    //    int startLevel = worldIndex * levelsPerWorld + 1;
    //    int endLevel = startLevel + levelsPerWorld - 1;

    //    for (int level = startLevel; level <= endLevel; level++)
    //    {
    //        GameObject btnObj = Instantiate(levelButtonPrefab, contentParent);
    //        LevelSelector selector = btnObj.GetComponent<LevelSelector>();

    //        bool unlocked = level <= unlockedLevel;
    //        int stars = PlayerPrefs.GetInt("LevelStars" + level, 0);

    //        selector.Setup(level, unlocked, stars);
    //    }
    //}
    void GenerateLevels()
    {
        ClearOldButtons();

        int worldIndex = PlayerPrefs.GetInt("SelectedWorld", 0);
        WorldData world = WorldDatabase.Instance.GetWorlds()
                                               .Find(w => w.worldId == worldIndex);

        // 🟢 APPLY WORLD THEME
        worldNameText.text = world.worldName;
        panelFrame.color = world.primaryColor;

        int levelsPerWorld = 10;
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        int startLevel = worldIndex * levelsPerWorld + 1;
        int endLevel = startLevel + levelsPerWorld - 1;

        for (int level = startLevel; level <= endLevel; level++)
        {
            GameObject btnObj = Instantiate(levelButtonPrefab, contentParent);
            LevelSelector selector = btnObj.GetComponent<LevelSelector>();

            bool unlocked = level <= unlockedLevel;
            int stars = PlayerPrefs.GetInt("LevelStars" + level, 0);

            selector.Setup(level, unlocked, stars, world.secondaryColor);
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
