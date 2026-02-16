using UnityEngine;

[CreateAssetMenu(menuName = "Game/World Data")]
public class WorldData : ScriptableObject
{
    [Header("Identity")]
    public int worldId;              // 1–5
    public string worldName;

    [Header("Progression")]
    public int starsRequired;

    [Header("Theme")]
    public Color primaryColor;       // panel frame, borders
    public Color secondaryColor;     // text outline, locks, empty stars

    [Header("Sprites")]
    public Sprite panelBackground;   // optional

}
