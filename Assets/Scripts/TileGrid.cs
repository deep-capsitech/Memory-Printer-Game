//using UnityEngine;

//public class TileGrid : MonoBehaviour
//{
//    public GameObject tilePrefab;
//    public Transform tileParent;
//    public int gridSize = 10;
//    public float tileSpacing = 1.2f;

//    public GameObject[,] tiles;

//    void Awake()
//    {
//        GenerateGrid();
//    }

//    public void GenerateGrid()
//    {
//        foreach (Transform t in tileParent)
//            Destroy(t.gameObject);

//        tiles = new GameObject[gridSize, gridSize];

//        for (int x = 0; x < gridSize; x++)
//        {
//            for (int z = 0; z < gridSize; z++)
//            {
//                GameObject tile = Instantiate(tilePrefab, tileParent);
//                tile.name = $"Tile_{x}_{z}";
//                tile.transform.localPosition = new Vector3(
//                    x * tileSpacing,
//                    0.02f,
//                    z * tileSpacing
//                );


//                tiles[x, z] = tile;

//                tile.SetActive(false);
//            }
//        }
//    }

//    public GameObject GetTile(int x, int z)
//    {
//        return tiles[x, z];
//    }

//    public Vector3 GetWorldPosition(int x, int z)
//    {
//        return tiles[x, z].transform.position;
//    }
//}

using UnityEngine;
using static UnityEngine.UI.Image;

public class TileGrid : MonoBehaviour
{
    public GameObject tilePrefab;
    public Transform tileParent;

    public int gridSize = 10;
    public float tileSpacing = 1.2f;

    public GameObject[,] tiles;


    void Awake()
    {
        GenerateGrid();
    }


    public void GenerateGrid()
    {
        foreach (Transform t in tileParent)
            Destroy(t.gameObject);

        tiles = new GameObject[gridSize, gridSize];

        float spacing = tileSpacing;
        Vector3 pos = Vector3.zero;

        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                GameObject tile = Instantiate(tilePrefab, tileParent);

                pos.x = x * spacing;
                pos.z = z * spacing;
                pos.y = 0.02f;

                tile.transform.localPosition = pos;

                tile.SetActive(false);
                tiles[x, z] = tile;
            }
        }
    }

    public Vector3 GetWorldPosition(int x, int z, float yOffset = 0.6f)
    {
        if (!IsValidTile(x, z))
        {
            Debug.LogError($"Invalid Tile Index: {x},{z}");
            return Vector3.zero;
        }

        Vector3 tilePos = tiles[x, z].transform.position;
        tilePos.y = yOffset;
        return tilePos;
    }

    // TILE CENTER (OPTIONAL)
    public Vector3 GetTileCenter(int x, int z, float yOffset = 0.6f)
    {
        float half = tileSpacing * 0.42f;

        Vector3 localPos = new Vector3(
            x * tileSpacing + half,
            yOffset,
            z * tileSpacing + half
        );
        return tileParent.TransformPoint(localPos);
    }

    public Vector3 GetNearestTileCenter(Vector3 worldPos)
    {
        Vector3 local = tileParent.InverseTransformPoint(worldPos);
        int x = Mathf.RoundToInt(local.x / tileSpacing);
        int z = Mathf.RoundToInt(local.z / tileSpacing);

        if (!IsValidTile(x, z))
            return transform.position;

        return GetTileCenter(x, z, 0.6f);
    }

    public bool TryGetTileIndex(Vector3 worldPos, out int x, out int z)
    {
        Vector3 local = tileParent.InverseTransformPoint(worldPos);

        x = Mathf.RoundToInt(local.x / tileSpacing);
        z = Mathf.RoundToInt(local.z / tileSpacing);

        return IsValidTile(x, z);
    }
    // SAFETY CHECK
    public bool IsValidTile(int x, int z)
    {
        return x >= 0 && x < gridSize && z >= 0 && z < gridSize;
    }
}
