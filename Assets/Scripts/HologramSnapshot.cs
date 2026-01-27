//using UnityEngine;
//using System.Collections.Generic;

//public class HologramSnapshot : MonoBehaviour
//{
//    public Material hologramMaterial;
//    public LevelGenerator generator;

//    private List<GameObject> holograms = new List<GameObject>();

//    public void TakeSnapshot()
//    {
//        ClearSnapshot();

//        foreach (Transform ob in generator.obstaclesParent)
//        {
//            CreateHologramCopy(ob);
//        }

//        foreach (Transform t in generator.tileGrid.tileParent)
//        {
//            CreateHologramCopy(t);
//        }
//    }

//    void CreateHologramCopy(Transform original)
//    {
//        GameObject ghost = Instantiate(original.gameObject);
//        ghost.transform.position = original.position;
//        ghost.transform.rotation = original.rotation;
//        ghost.transform.localScale = original.localScale;

//        Renderer r = ghost.GetComponent<Renderer>();
//        if (r) r.material = hologramMaterial;

//        holograms.Add(ghost);
//    }

//    public void ClearSnapshot()
//    {
//        foreach (var h in holograms)
//        {
//            Destroy(h);
//        }
//        holograms.Clear();
//    }
//}
using UnityEngine;
using System.Collections.Generic;

public class HologramSnapshot : MonoBehaviour
{
    public Material hologramMaterial;
    public LevelGenerator generator;

    private readonly List<GameObject> holograms = new List<GameObject>();


    public void TakeSnapshot()
    {
        ClearSnapshot();

        foreach (Transform ob in generator.obstaclesParent)
            CreateHologramCopy(ob);
    }


    void CreateHologramCopy(Transform original)
    {
        MeshFilter srcMesh = original.GetComponent<MeshFilter>();
        MeshRenderer srcRenderer = original.GetComponent<MeshRenderer>();

        if (!srcMesh || !srcRenderer)
            return;

        GameObject ghost = new GameObject("GhostObstacle");
        ghost.transform.SetPositionAndRotation(original.position, original.rotation);
        ghost.transform.localScale = original.localScale;
        ghost.transform.SetParent(transform, false);

        MeshFilter f = ghost.AddComponent<MeshFilter>();
        f.sharedMesh = srcMesh.sharedMesh;

        MeshRenderer r = ghost.AddComponent<MeshRenderer>();
        r.sharedMaterial = hologramMaterial;

        holograms.Add(ghost);
    }


    public void ClearSnapshot()
    {
        for (int i = 0; i < holograms.Count; i++)
            if (holograms[i] != null) Destroy(holograms[i]);

        holograms.Clear();
    }
}
