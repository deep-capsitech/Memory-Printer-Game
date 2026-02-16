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
        WorldData world = WorldDatabase.Instance.GetWorlds()
            .Find(w => w.worldId == worldId);

        if (world == null) return;

        // 🔹 WORLD TITLE
        worldNameText.text = world.worldName;
        worldNameText.color = Color.white;

        // IMPORTANT: TMP MATERIAL INSTANCE
        worldNameText.fontMaterial = Instantiate(worldNameText.fontMaterial);
        worldNameText.fontMaterial.SetColor("_OutlineColor", world.primaryColor);

        // 🔹 PANEL FRAME
        panelFrame.color = world.primaryColor;
        if (world.panelBackground != null)
            panelFrame.sprite = world.panelBackground;

        // 🔹 BACK BUTTON (WHITE SPRITE REQUIRED)
        backButtonImage.color = world.primaryColor;

        // 🔹 LEVEL RANGE
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        int startLevel = (worldId - 1) * levelsPerWorld + 1;
        int endLevel = startLevel + levelsPerWorld - 1;

        for (int level = startLevel; level <= endLevel; level++)
        {
            GameObject btn = Instantiate(levelButtonPrefab, contentParent);
            LevelSelector selector = btn.GetComponent<LevelSelector>();

            bool unlocked = level <= unlockedLevel;
            int stars = PlayerPrefs.GetInt("LevelStars" + level, 0);

            selector.Setup(level, unlocked, stars, world);
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
