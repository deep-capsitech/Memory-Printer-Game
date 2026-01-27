
//using System.Collections.Generic;
//using UnityEngine;

//public class LevelGenerator : MonoBehaviour
//{
//    public GameObject obstaclePrefab;
//    public Transform obstaclesParent;

//    public int obstacleCount = 12;

//    public TileGrid tileGrid;
//    public Transform player;
//    public Transform door;

//    [System.Serializable]
//    public class ObstacleData
//    {
//        public Vector3 position;
//        public Quaternion rotation;
//    }

//    public List<ObstacleData> savedLayout = new List<ObstacleData>();

//    public void GenerateNewLayout()
//    {
//        ClearObstacles();
//        savedLayout.Clear();

//        int count = 0;
//        float spacing = tileGrid.tileSpacing;
//        float half = spacing * 0.5f;

//        Vector3 origin = tileGrid.tileParent.position;

//        while (count < obstacleCount)
//        {
//            int x = Random.Range(0, tileGrid.gridSize);
//            int z = Random.Range(0, tileGrid.gridSize);

//            Vector3 pos = origin + new Vector3(x * spacing + half, 0.6f, z * spacing + half);

//            if ((pos - player.position).sqrMagnitude < 4f) continue;
//            if ((pos - door.position).sqrMagnitude < 4f) continue;

//            GameObject ob = Instantiate(obstaclePrefab, pos, Quaternion.identity, obstaclesParent);

//            savedLayout.Add(new ObstacleData
//            {
//                position = pos,
//                rotation = ob.transform.rotation
//            });

//            count++;
//        }
//    }

//    public bool IsPositionDangerous(Vector3 playerPos)
//    {
//        float spacing = tileGrid.tileSpacing;

//        int px = Mathf.RoundToInt(playerPos.x / spacing);
//        int pz = Mathf.RoundToInt(playerPos.z / spacing);

//        foreach (Transform ob in obstaclesParent)
//        {
//            int ox = Mathf.RoundToInt(ob.position.x / spacing);
//            int oz = Mathf.RoundToInt(ob.position.z / spacing);

//            if (px == ox && pz == oz)
//                return true;
//        }
//        return false;
//    }


//    public void RestoreLayout()
//    {
//        ClearObstacles();

//        foreach (ObstacleData data in savedLayout)
//        {
//            Instantiate(
//                obstaclePrefab,
//                data.position,
//                data.rotation,
//                obstaclesParent
//            );
//        }
//    }

//    void ClearObstacles()
//    {
//        foreach (Transform t in obstaclesParent)
//            Destroy(t.gameObject);
//    }


//}


using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public GameObject boosterPrefab;
    public Transform obstaclesParent;
    public Transform boosterParent;
    public TileGrid tileGrid;

    public float obstacleYOffset = 0.6f;
    public float boosterYOffset = 0.6f;

    public float boosterSpawnDelay = 5f;
    public float boosterLifeTime = 5f;

    JsonLayout currentLayout;

    public void GenerateFromJson(int levelNumber, int layoutIndex)
    {
        if (JsonLevelLoader.Instance == null)
        {
            Debug.LogError("JsonLevelLoader Instance is NULL");
            return;
        }

        if (tileGrid == null)
        {
            Debug.LogError("TileGrid is NOT assigned");
            return;
        }

        DestroyAllObstacles();
        DestroyBooster();

        JsonLevel levelData = JsonLevelLoader.Instance.GetLevel(levelNumber);

        if (levelData == null)
        {
            Debug.LogError("Level not found: " + levelNumber);
            return;
        }

        if (layoutIndex >= levelData.layouts.Count)
        {
            Debug.LogError("Layout index out of range");
            return;
        }

        currentLayout = levelData.layouts[layoutIndex];

        // Spawn Obstacles
        foreach (var ob in currentLayout.obstacles)
        {
            Vector3 pos = tileGrid.GetTileCenter(
                ob.tileX,
                ob.tileZ,
                obstacleYOffset
            );

            Instantiate(obstaclePrefab, pos, Quaternion.identity, obstaclesParent);
        }

        // Spawn Booster
        StartCoroutine(SpawnBoosterAfterDelay());
       
    }


    IEnumerator SpawnBoosterAfterDelay()
    {
        yield return new WaitForSeconds(boosterSpawnDelay);

        if (currentLayout == null || currentLayout.booster == null)
            yield break;

        Vector3 boosterPos = tileGrid.GetTileCenter(
            currentLayout.booster.tileX,
            currentLayout.booster.tileZ,
            boosterYOffset
        );

        GameObject booster = Instantiate(
            boosterPrefab,
            boosterPos,
            Quaternion.identity,
            boosterParent
        );

        Destroy(booster, boosterLifeTime);
    }

    public void DestroyAllObstacles()
    {
        foreach (Transform t in obstaclesParent)
            Destroy(t.gameObject);
    }

    public void DestroyBooster() { 
        foreach (Transform t in boosterParent)
            Destroy(t.gameObject);
    }


    public void EnableDragMode(bool enable)
    {
        foreach (Transform ob in obstaclesParent)
        {
            DraggableObstacle d = ob.GetComponent<DraggableObstacle>();
            if (d != null)
                d.EnableDrag(enable);
        }
    }

}

