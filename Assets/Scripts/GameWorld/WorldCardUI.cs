using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldCardUI : MonoBehaviour
{
    public GameObject lockIcon;
    public Button button;
    public TextMeshProUGUI worldNameText;

    int worldId;

    public void Setup(WorldData data, int totalStars)
    {
        worldId = data.worldId;
        worldNameText.text = data.worldName;

        bool unlocked =
            data.worldId == 0 ||          // ✅ World 1 always unlocked
            totalStars >= data.starsRequired;

        lockIcon.SetActive(!unlocked);
        button.interactable = unlocked;

        Debug.Log($"WORLD {data.worldId} | Required: {data.starsRequired} | TotalStar: {totalStars}");
    }

    public void OnClick()
    {
        PlayerPrefs.SetInt("SelectedWorld", worldId);   // ✅ FIX
        PlayerPrefs.Save();

        WorldClickHandler.Instance.OnWorldSelected(worldId);
    }

}
