using UnityEngine;

[CreateAssetMenu(menuName = "Game/World Data")]
public class WorldData : ScriptableObject
{
    [Header("Identity")]
    public int worldId;
    public string worldName;

    [Header("Progression")]
    public int starsRequired;

    [Header("Theme")]
    public Color primaryColor;
}