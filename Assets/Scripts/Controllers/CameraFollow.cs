using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float followSpeed = 6f;
    public float offsetSmoothSpeed = 3f;

    [Header("Offsets")]
    public Vector3 defaultOffset;
    public Vector3 snapshotOffset;
    public Vector3 gameplayOffset;
    public Vector3 winOffset;

    private Vector3 currentOffset;
    private Vector3 targetOffset;

    void Start()
    {
        currentOffset = defaultOffset;
        targetOffset = defaultOffset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        currentOffset = Vector3.Lerp(currentOffset, targetOffset, offsetSmoothSpeed * Time.unscaledDeltaTime);

        Vector3 desiredPos = target.position + currentOffset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.unscaledDeltaTime);
    }

    public void SetDefault()
    {
        targetOffset = defaultOffset;
    }

    public void SetSnapshotView()
    {
        targetOffset = snapshotOffset;
    }

    public void SetGameplayView()
    {
        targetOffset = gameplayOffset;
    }

    public void SetWinView()
    {
        targetOffset = winOffset;
    }
}