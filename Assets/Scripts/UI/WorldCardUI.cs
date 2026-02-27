using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class WorldCardUI : MonoBehaviour
{
    public GameObject lockIcon;
    public Button button;
    public TextMeshProUGUI worldNameText;

    private int worldId;

    public void Setup(WorldData data, int totalStars)
    {
        worldId = data.worldId;
        worldNameText.text = data.worldName;

        bool unlocked =
            data.worldId == 1 ||
            totalStars >= data.starsRequired;

        lockIcon.SetActive(!unlocked);
        button.interactable = unlocked;
    }

    public void OnClick()
    {
        GameManagerCycle.Instance.OnWorldSelected(worldId);
    }
}