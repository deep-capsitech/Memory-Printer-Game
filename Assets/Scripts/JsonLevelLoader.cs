using UnityEngine;

public class JsonLevelLoader : MonoBehaviour
{
    public static JsonLevelLoader Instance;

    public JsonLevelData gameData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadJson();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("levels");

        if (jsonFile == null)
        {
            Debug.LogError("levels.json NOT found in Resources folder!");
            return;
        }

        gameData = JsonUtility.FromJson<JsonLevelData>(jsonFile.text);
        Debug.Log("Levels Loaded: " + gameData.levels.Count);
    }

    public JsonLevel GetLevel(int level)
    {
        return gameData.levels.Find(l => l.level == level);
    }
}
