using UnityEngine;
using System.Collections.Generic;

public class WorldDatabase : MonoBehaviour
{
    public static WorldDatabase Instance;

    [Header("All Worlds")]
    public List<WorldData> worlds;

    [Header("Current World")]
    public int currentWorldIndex = 0;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public List<WorldData> GetWorlds()
    {
        return worlds;
    }

    public WorldData GetCurrentWorld()
    {
        if (worlds == null || worlds.Count == 0)
            return null;

        return worlds[Mathf.Clamp(currentWorldIndex, 0, worlds.Count - 1)];
    }
}