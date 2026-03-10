using UnityEngine;
using System.Collections.Generic;

public class WorldPanelController : MonoBehaviour
{
    public List<WorldCardUI> worldCards;

    void OnEnable()
    {
        RefreshWorlds();
    }

    void RefreshWorlds()
    {
        int totalStars = PlayerPrefs.GetInt("TotalStar", 0);
        List<WorldData> worlds = WorldDatabase.Instance.GetWorlds();

        for (int i = 0; i < worldCards.Count; i++)
        {
            worldCards[i].Setup(worlds[i], totalStars);
        }
    }

    public void OnBackButtonClicked()
    {
        GameManagerCycle.Instance.ShowMenu();
    }
}
