
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

    public void MoveGhost(Transform original)
    {
        for (int i = 0; i < ghosts.Count; i++)
        {
            if (ghosts[i] == null) continue;

            // Ghost same index pe hai jaha original obstacle tha
            if (i < generator.obstaclesParent.childCount)
            {
                Transform real = generator.obstaclesParent.GetChild(i);

                if (real == original)
                {
                    ghosts[i].transform.position =
                        original.position + Vector3.up * 0.05f;
                }
            }
        }
    }
}
