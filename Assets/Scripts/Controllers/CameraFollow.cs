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

    [Header("Zoom Settings")]
    public Camera cam;
    public float defaultFOV = 60f;
    public float snapshotFOV = 45f;
    public float fovSmoothSpeed = 5f;

    private float targetFOV;

    void Start()
    {
        currentOffset = defaultOffset;
        targetOffset = defaultOffset;
        targetFOV = defaultFOV;
        if (cam != null)
            cam.fieldOfView = defaultFOV;
    }

    void LateUpdate()
    {
        if (target == null) return;

        currentOffset = Vector3.Lerp(currentOffset, targetOffset, offsetSmoothSpeed * Time.unscaledDeltaTime);

        Vector3 desiredPos = target.position + currentOffset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.unscaledDeltaTime);

        if (cam != null)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovSmoothSpeed * Time.unscaledDeltaTime);
        }
    }

    public void SetDefault()
    {
        targetOffset = defaultOffset;
        targetFOV = defaultFOV;
    }

    public void SetSnapshotView()
    {
        targetOffset = snapshotOffset;
        targetFOV = snapshotFOV;
    }

    public void SetGameplayView()
    {
        targetOffset = gameplayOffset;
        targetFOV = defaultFOV;
    }

    public void SetWinView()
    {
        targetOffset = winOffset;
        targetFOV = 60f;
    }
}