using UnityEngine;

public class WorldThemeController : MonoBehaviour
{
    [Header("Room (Walls, Roof, etc)")]
    public Renderer[] roomParts;

    [Header("Floor")]
    public Renderer[] floorParts; // 👈 ADD THIS

    [Header("Line Renderers (Tile + Door)")]
    public LineRenderer[] lineParts;

    public void ApplyWorldTheme(WorldData world)
    {
        // ROOM
        foreach (var r in roomParts)
        {
            r.material = world.roomMaterial;
        }

        // FLOOR (NEW)
        foreach (var r in floorParts)
        {
            r.material = world.floorMaterial; // or separate floorMaterial if you want
        }

        // TILE + DOOR OUTLINES
        foreach (var l in lineParts)
        {
            l.material = world.hologramMaterial;
        }
    }
}