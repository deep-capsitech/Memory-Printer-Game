using UnityEngine;
using System.Collections;


public class MovingObstacle : MonoBehaviour
{
    public enum MoveType
    {
        None,
        UpDown,
        LeftRight,
        Both
    }

    [Header("Movement")]
    public bool canMove = false;
    public float moveDistance = 1.5f;
    public float moveSpeed = 1.2f;

    private Vector3 startPos;
    private Vector3 moveAxis;

    [Header("Materials")]
    public Material defaultMat;
    public Material glowMat; 

    private MeshRenderer rend;

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
        //moveAxis = (Random.value > 0.5f) ? Vector3.right : Vector3.forward;
    }

    void Update()
    {
        if (!canMove) return;

        float offset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = startPos + moveAxis * offset;
    }

    public void StartWarningGlow()
    {
        if (!gameObject.activeInHierarchy || rend ==null)
        {
            return;
        }

        if (glowMat == null)
        {
            canMove = true;
            return;
        }
        rend.enabled = true;

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