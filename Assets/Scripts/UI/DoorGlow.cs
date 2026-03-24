using UnityEngine;

public class DoorGlow : MonoBehaviour
{
    public static DoorGlow Instance;

    public Renderer doorRenderer;

    [Header("Fallback")]
    public Material defaultMaterial;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ApplyDefault();
    }
    public void ApplyWorldMaterial()
    {
        if (WorldDatabase.Instance == null)
        {
            Debug.LogWarning("WorldDatabase not found!");
            ApplyDefault();
            return;
        }

        WorldData world = WorldDatabase.Instance.GetCurrentWorld();

        if (world != null && world.hologramMaterial != null)
        {
            doorRenderer.material = world.hologramMaterial;
            Debug.Log("Door material applied from world: " + world.name);
        }
        else
        {
            Debug.LogWarning("World or material missing, using default");
            ApplyDefault();
        }
    }

    public void ApplyDefault()
    {
        if (doorRenderer != null && defaultMaterial != null)
            doorRenderer.material = defaultMaterial;
    }
}