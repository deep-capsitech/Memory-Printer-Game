using UnityEngine;

public class WorldClickHandler : MonoBehaviour
{
    public static WorldClickHandler Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void OnWorldSelected(int worldId)
    {
        PlayerPrefs.SetInt("SelectedWorld", worldId);
        PlayerPrefs.Save();

        GameManagerCycle.Instance.OpenLevelPanel();
    }

}
