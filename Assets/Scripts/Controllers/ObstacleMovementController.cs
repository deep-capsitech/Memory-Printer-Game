using System.Collections.Generic;
using UnityEngine;

public class ObstacleMovementController : MonoBehaviour
{
    [Header("Dependencies")]
    public LevelGenerator generator;
    public GameStateController gameStateController;

    private HashSet<MovingObstacle> movingObstaclesForLayout = new HashSet<MovingObstacle>();

    private bool snapshotActive;
    private bool powerUpActive;
    private bool freezeActive;

    private MovingObstacle.MoveType currentMoveType;
    // =========================================================
    // INITIALIZE FOR NEW LAYOUT
    // =========================================================
    public void InitializeLayout(int levelIndex)
    {
        movingObstaclesForLayout.Clear();

        List<MovingObstacle> allObstacles = new List<MovingObstacle>();
        List<MovingObstacle> eligible = new List<MovingObstacle>();

        foreach (Transform ob in generator.obstaclesParent)
        {
            MovingObstacle mo = ob.GetComponent<MovingObstacle>();
            if (mo == null) continue;

            mo.ForceStopMovement();
            allObstacles.Add(mo);
        }

        if (allObstacles.Count == 0) return;
        currentMoveType = GetMoveType(levelIndex);
        MovingObstacle.MoveType moveType = currentMoveType;
        //MovingObstacle.MoveType moveType = GetMoveType(levelIndex);
        if (moveType == MovingObstacle.MoveType.None) return;

        foreach (var mo in allObstacles)
        {
            if (IsValidForMoveType(mo, moveType))
                eligible.Add(mo);
        }

        if (eligible.Count == 0) return;

        Shuffle(eligible);

        int moveCount = Mathf.Max(1, allObstacles.Count / 2);
        moveCount = Mathf.Min(moveCount, eligible.Count);

        for (int i = 0; i < moveCount; i++)
            movingObstaclesForLayout.Add(eligible[i]);

        ApplyStoredMovementRules();
    }

    // =========================================================
    // MOVEMENT CONTROL
    // =========================================================
    public void StopAll()
    {
        foreach (Transform ob in generator.obstaclesParent)
        {
            MovingObstacle mo = ob.GetComponent<MovingObstacle>();
            if (mo != null)
                mo.ForceStopMovement();
        }
    }
    public void ApplyStoredMovementRules()
    {
        if (IsMovementBlocked())
        {
            StopAll();
            return;
        }

        foreach (Transform ob in generator.obstaclesParent)
        {
            MovingObstacle mo = ob.GetComponent<MovingObstacle>();
            if (mo == null) continue;

            mo.ForceStopMovement();

            if (movingObstaclesForLayout.Contains(mo))
            {
                mo.SetMovementType(currentMoveType);
                mo.StartWarningGlow();
            }
        }
    }

    // =========================================================
    // STATE HOOKS
    // =========================================================
    public void OnSnapshotStart()
    {
        snapshotActive = true;
        StopAll();
    }

    public void OnSnapshotEnd()
    {
        snapshotActive = false;
        ApplyStoredMovementRules();
    }

    public void OnFreezeStart()
    {
        freezeActive = true;
        StopAll();
    }

    public void OnFreezeEnd()
    {
        freezeActive = false;
        ApplyStoredMovementRules();
    }

    public void OnPowerUpStart()
    {
        powerUpActive = true;
        StopAll();
    }

    public void OnPowerUpEnd()
    {
        powerUpActive = false;
        ApplyStoredMovementRules();
    }

    // =========================================================
    // INTERNAL LOGIC
    // =========================================================
    bool IsMovementBlocked()
    {
        return !gameStateController.IsGameplayActive()
               || snapshotActive
               || powerUpActive
               || freezeActive;
    }

    MovingObstacle.MoveType GetMoveType(int levelIndex)
    {
        if (levelIndex >= 11 && levelIndex <= 20)
            return MovingObstacle.MoveType.UpDown;

        if (levelIndex >= 21 && levelIndex <= 30)
            return MovingObstacle.MoveType.LeftRight;

        if (levelIndex >= 31 && levelIndex <= 40)
            return MovingObstacle.MoveType.Both;

        if (levelIndex >= 41 && levelIndex <= 50)
            return MovingObstacle.MoveType.Square;

        return MovingObstacle.MoveType.None;
    }

    bool IsValidForMoveType(MovingObstacle mo, MovingObstacle.MoveType moveType)
    {
        if (moveType == MovingObstacle.MoveType.UpDown)
            return !(mo.tileZ == 0 || mo.tileZ == 9);

        if (moveType == MovingObstacle.MoveType.LeftRight)
            return !(mo.tileX == 0 || mo.tileX == 9);

        if (moveType == MovingObstacle.MoveType.Both)
            return !(mo.tileX == 0 || mo.tileX == 9 ||
                     mo.tileZ == 0 || mo.tileZ == 9);

        return true;
    }

    void Shuffle(List<MovingObstacle> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
    // =========================================================
    // GAME OVER / RESET
    // =========================================================
    public void OnGameOver()
    {
        snapshotActive = false;
        powerUpActive = false;
        freezeActive = false;

        StopAll();
    }

    public void ResetState()
    {
        snapshotActive = false;
        powerUpActive = false;
        freezeActive = false;

        StopAll();
    }
}