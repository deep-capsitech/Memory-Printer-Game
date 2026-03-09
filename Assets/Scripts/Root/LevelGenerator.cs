
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

    public float boosterSpawnDelay = 10f;
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
        StopAllCoroutines();
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

           GameObject obstacleObj = Instantiate(obstaclePrefab, pos, Quaternion.identity, obstaclesParent);

            MovingObstacle mo = obstacleObj.GetComponent<MovingObstacle>();

            if (mo != null)
            {
                mo.tileX = ob.tileX;
                mo.tileZ = ob.tileZ;
                mo.InitializeFromGrid();
            }
        }

        bool tutorialCompleted = PlayerPrefs.GetInt("TutorialDone", 0) == 1;

        // Spawn Booster
        //StartCoroutine(SpawnBoosterAfterDelay());
        // Spawn Booster ONLY if unlocked (Level 21+)
        if ((levelNumber == 1 && !tutorialCompleted) || levelNumber >= 21)
        {
            StartCoroutine(SpawnBoosterAfterDelay());
        }
    }

    IEnumerator SpawnBoosterAfterDelay()
    {
        yield return new WaitForSeconds(boosterSpawnDelay);
        SpawnBoosterNow();
    }
   
    public void SpawnBoosterNow()
    {
        if (currentLayout == null || currentLayout.booster == null)
            return;

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

        bool tutorialActive =
            TutorialManager.Instance != null &&
            TutorialManager.Instance.isTutorialActive;

        if (tutorialActive)
        {
            TutorialManager.Instance.OnBoosterAppeared();
        }
        else
        {
            Destroy(booster, boosterLifeTime);
        }
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

