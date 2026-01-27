using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float followSpeed = 6f;
    public Vector3 offset;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
    }
}
