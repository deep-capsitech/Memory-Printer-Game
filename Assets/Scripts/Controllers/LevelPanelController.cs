using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelPanelController : MonoBehaviour
{
    [Header("Panel UI")]
    public TextMeshProUGUI worldNameText;
    public Image panelFrame;
    public Image backButtonImage;

    [Header("Level Grid")]
    public Transform contentParent;
    public GameObject levelButtonPrefab;

    [Header("Config")]
    public int levelsPerWorld = 10;

    void OnEnable()
    {
        BuildPanel();
    }
    void BuildPanel()
    {
        ClearOldButtons();

        int worldId = PlayerPrefs.GetInt("SelectedWorld", 1);
        int totalStars = PlayerPrefs.GetInt("TotalStar", 0);

        WorldData world = WorldDatabase.Instance.GetWorlds()
            .Find(w => w.worldId == worldId);

        if (world == null) return;

        // WORLD TITLE
        worldNameText.text = world.worldName;
        worldNameText.color = Color.white;
        worldNameText.fontMaterial = Instantiate(worldNameText.fontMaterial);
        worldNameText.fontMaterial.SetColor("_OutlineColor", world.primaryColor);

        panelFrame.color = world.primaryColor;
        if (world.panelBackground != null)
            panelFrame.sprite = world.panelBackground;

        backButtonImage.color = world.primaryColor;

        bool worldUnlocked =
            worldId == 1 ||
            totalStars >= world.starsRequired;

        int startLevel = (worldId - 1) * levelsPerWorld + 1;
        int endLevel = startLevel + levelsPerWorld - 1;

        for (int level = startLevel; level <= endLevel; level++)
        {
            GameObject btn = Instantiate(levelButtonPrefab, contentParent);
            LevelSelector selector = btn.GetComponent<LevelSelector>();

            bool levelUnlocked = false;

            if (!worldUnlocked)
            {
                levelUnlocked = false;
            }
            else if (level == startLevel)
            {
                levelUnlocked = true;
            }
            else
            {
                int previousStars = PlayerPrefs.GetInt("LevelStars" + (level - 1), 0);
                levelUnlocked = previousStars > 0;
            }

            int stars = PlayerPrefs.GetInt("LevelStars" + level, 0);

            selector.Setup(level, levelUnlocked, stars, world);
        }
    }
    void ClearOldButtons()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);
    }

    public void OnBackClicked()
    {
        GameManagerCycle.Instance.uiFlowController.OpenWorldLevels();
    }
}
