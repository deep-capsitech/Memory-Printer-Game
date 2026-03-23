using UnityEngine;

[CreateAssetMenu(menuName = "Game/World Data")]
public class WorldData : ScriptableObject
{
    [Header("Identity")]
    public int worldId;
    public string worldNameKey;

    [Header("Progression")]
    public int starsRequired;

    [Header("Theme")]
    public Color primaryColor;
    public Material roomMaterial;
    public Material hologramMaterial;
    public Material floorMaterial;
}