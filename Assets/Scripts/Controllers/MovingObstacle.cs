using UnityEngine;
using System.Collections;


public class MovingObstacle : MonoBehaviour
{
    public enum MoveType
    {
        None,
        UpDown,
        LeftRight,
        Both,
        Square
    }

    [Header("Movement")]
    public bool canMove = false;
    public float moveDistance = 1.5f;
    public float moveSpeed = 2f;

    private Vector3 startPos;
    private Vector3 moveAxis;

    [Header("Materials")]
    public Material defaultMat;
    public Material glowMat;

    private MeshRenderer rend;

    private int squareStep = 0;
    private Vector3 squareStartPos;
    private float squareSize;
    private int squareDirection = 1;
    private Vector3 originalStartPos;

    [Header("Grid Settings")]
    public float tileSize = 1.2f;
    public int minBorderIndex = 1;
    public int maxBorderIndex = 8;

    [HideInInspector] public int tileX;
    [HideInInspector] public int tileZ;

    private MoveType currentMoveType = MoveType.None;

    void Start()
    {
        rend = GetComponentInChildren<MeshRenderer>();
        if (rend == null)
        {
            enabled = false;
            return;
        }

        defaultMat = rend.material;
        startPos = transform.position;
        originalStartPos = transform.position;
        squareStartPos = originalStartPos;
        CalculateSquareSize();
        //moveAxis = (Random.value > 0.5f) ? Vector3.right : Vector3.forward;
    }

    void Update()
    {
        if (!canMove) return;

        if (currentMoveType == MoveType.Square)
        {
            UpdateSquareMovement();
            return;
        }

        float offset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = startPos + moveAxis * offset;
    }

    void CalculateSquareSize()
    {
        // check border condition
        bool isBorder =
            tileX == minBorderIndex ||
            tileX == maxBorderIndex ||
            tileZ == minBorderIndex ||
            tileZ == maxBorderIndex;

        // assign square size
        squareSize = isBorder ? tileSize : tileSize * 2f;
    }

    public void InitializeFromGrid()
    {
        originalStartPos = transform.position;
        squareStartPos = originalStartPos;

        CalculateSquareSize();
    }
    void UpdateSquareMovement()
    {
        Vector3 targetPos = GetSquareTarget(squareStep);

        // move with constant speed
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            //squareStep = (squareStep + 1) % 4;
            squareStep += squareDirection;

            if (squareStep > 3) squareStep = 0;
            if (squareStep < 0) squareStep = 3;
        }
    }

    Vector3 GetSquareTarget(int step)
    {
        switch (step)
        {
            case 0: return squareStartPos + Vector3.right * squareSize;
            case 1: return squareStartPos + (Vector3.right + Vector3.forward) * squareSize;
            case 2: return squareStartPos + Vector3.forward * squareSize;
            case 3: return squareStartPos;
        }
        return squareStartPos; ;
    }

    public void StartWarningGlow()
    {
        if (!gameObject.activeInHierarchy || rend == null)
        {
            return;
        }

        if (glowMat == null)
        {
            canMove = true;
            return;
        }
        rend.enabled = true;

        squareStartPos = originalStartPos;
        squareStep = 0;
        squareDirection = (transform.position.x > 0) ? 1 : -1;
        StopAllCoroutines();
        StartCoroutine(GlowRoutine());
    }

    public void SetMovementType(MoveType type)
    {
        currentMoveType = type;

        switch (type)
        {
            case MoveType.UpDown:
                moveAxis = Vector3.forward;
                break;

            case MoveType.LeftRight:
                moveAxis = Vector3.right;
                break;

            case MoveType.Both:
                moveAxis = (Random.value > 0.5f)
                    ? Vector3.right
                    : Vector3.forward;
                break;

            default:
                moveAxis = Vector3.zero;
                break;
        }
    }

    IEnumerator GlowRoutine()
    {
        rend.material = glowMat;
        canMove = false;

        yield return new WaitForSeconds(0.5f);
        canMove = true;
    }

    public void ForceStopMovement()
    {
        canMove = false;

        if (rend != null && defaultMat != null)
            rend.material = defaultMat;

    }
}