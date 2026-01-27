//using UnityEngine;
//using System.Collections.Generic;

//public class SnapshotManager : MonoBehaviour
//{
//    public LevelGenerator generator;
//    public TileGrid tileGrid;

//    public Material hologramMaterial;

//    private List<GameObject> ghosts = new List<GameObject>();

//    public void TakeSnapshot()
//    {
//        ClearSnapshot();

//        foreach (GameObject t in tileGrid.tiles)
//            t.SetActive(true);

//        foreach (Transform ob in generator.obstaclesParent)
//        {
//            GameObject ghost = new GameObject("GhostObstacle");
//            ghost.transform.position = ob.position + Vector3.up * 0.05f;
//            ghost.transform.rotation = ob.rotation;
//            ghost.transform.localScale = ob.localScale;
//            ghost.transform.parent = transform;

//            MeshFilter mf = ghost.AddComponent<MeshFilter>();
//            MeshRenderer mr = ghost.AddComponent<MeshRenderer>();

//            mf.sharedMesh = ob.GetComponent<MeshFilter>().sharedMesh;
//            mr.sharedMaterial = hologramMaterial;

//            ghosts.Add(ghost);
//        }
//    }

//    public void ClearSnapshot()
//    {
//        ClearHologram();

//        foreach (GameObject t in tileGrid.tiles)
//            t.SetActive(false);
//    }

//    public void ClearHologram()
//    {
//        foreach (var g in ghosts)
//            if (g != null) Destroy(g);

//        ghosts.Clear();
//    }
//}
using UnityEngine;
using System.Collections.Generic;

public class SnapshotManager : MonoBehaviour
{
    public LevelGenerator generator;
    public TileGrid tileGrid;
    public Material hologramMaterial;

    private readonly List<GameObject> ghosts = new List<GameObject>();


    public void TakeSnapshot()
    {
        ClearSnapshot();

        foreach (var tile in tileGrid.tiles)
            tile.SetActive(true);

        foreach (Transform ob in generator.obstaclesParent)
        {
            MeshFilter srcMesh = ob.GetComponent<MeshFilter>();
            if (!srcMesh) continue;

            GameObject ghost = new GameObject("GhostObstacle");
            ghost.transform.SetPositionAndRotation(
                ob.position + Vector3.up * 0.05f,
                ob.rotation
            );
            ghost.transform.localScale = ob.localScale;
            ghost.transform.SetParent(transform, false);

            MeshFilter mf = ghost.AddComponent<MeshFilter>();
            mf.sharedMesh = srcMesh.sharedMesh;

            MeshRenderer mr = ghost.AddComponent<MeshRenderer>();
            mr.sharedMaterial = hologramMaterial;

            ghosts.Add(ghost);
        }
    }


    public void ClearSnapshot()
    {
        for (int i = 0; i < ghosts.Count; i++)
            if (ghosts[i]) Destroy(ghosts[i]);

        ghosts.Clear();
        
        foreach (var tile in tileGrid.tiles)
            tile.SetActive(false);
    }
}
