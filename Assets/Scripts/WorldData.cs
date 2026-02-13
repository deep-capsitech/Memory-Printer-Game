using UnityEngine;

[CreateAssetMenu(menuName = "Game/World Data")]
public class WorldData : ScriptableObject
{
    [Header("Identity")]
    public int worldId;              // 0–4
    public string worldName;

    [Header("Progression")]
    public int starsRequired;

    [Header("Theme")]
    public Color primaryColor;       // glow / borders
    public Color secondaryColor;     // buttons / accents
    public Sprite panelBackground;   // optional (frame/bg)
}
