using UnityEngine;

public class DraggableObstacle : MonoBehaviour
{
    private Camera cam;
    private TileGrid grid;
    private Vector3 startPos;
    private bool dragging;

    void Start()
    {
        cam = Camera.main;
        grid = FindAnyObjectByType<TileGrid>();
    }

    void OnMouseDown()
    {
        if (!GameManagerCycle.Instance) return;
        if (!GameManagerCycle.Instance.enabled) return;

        if (!GameManagerCycle.Instance) return;
        if (!GameManagerCycle.Instance.enabled) return;

        dragging = true;
        startPos = transform.position;
    }

    void OnMouseDrag()
    {
        if (!dragging) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 snapPos = grid.GetNearestTileCenter(hit.point);
            if (snapPos != Vector3.zero)
                transform.position = snapPos;
        }
    }

    void OnMouseUp()
    {
        dragging = false;

        // Check if tile already has obstacle
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            0.2f
        );

        foreach (var h in hits)
        {
            if (h != null && h.gameObject != gameObject &&
                h.CompareTag("Obstacle"))
            {
                transform.position = startPos; // revert
                return;
            }
        }
    }
}
