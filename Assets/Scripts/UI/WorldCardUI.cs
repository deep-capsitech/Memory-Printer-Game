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

        bool unlocked;

        if (data.worldId == 1)
        {
            unlocked = true;
        }
        else
        {
            unlocked = PlayerPrefs.GetInt($"WorldUnlocked_{data.worldId}", 0) == 1;
        }

        lockIcon.SetActive(!unlocked);
        button.interactable = unlocked;
    }

    public void OnClick()
    {
        GameManagerCycle.Instance.OnWorldSelected(worldId);
    }
}