using UnityEngine;
public class DraggableObstacle : MonoBehaviour
{
    private bool canDrag;
    private bool dragging;

    private Vector3 startPos;
    private float yOffset;

    private TileGrid grid;

    void Start()
    {
        grid = FindAnyObjectByType<TileGrid>();
        yOffset = transform.position.y;
    }

    public void EnableDrag(bool enable)
    {
        canDrag = enable;
        dragging = false;
    }

    void OnMouseDown()
    {
        if (!canDrag) return;

        dragging = true;
        startPos = transform.position;
    }

    void OnMouseDrag()
    {
        if (!dragging) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float d))
        {
            Vector3 hit = ray.GetPoint(d);
            Vector3 snap = grid.GetNearestTileCenter(hit);

            transform.position = new Vector3(
                snap.x,
                yOffset,
                snap.z
            );
        }
    }

    void OnMouseUp()
    {
        if (!dragging) return;
        dragging = false;

        // Snap to nearest tile
        Vector3 snappedPos = grid.GetNearestTileCenter(transform.position);

        // Convert snapped world position tile index
        if (!grid.TryGetTileIndex(snappedPos, out int x, out int z))
        {
            // Outside grid
            transform.position = startPos;
            return;
        }

        // Check if another obstacle already exists on this tile
        Collider[] hits = Physics.OverlapSphere(snappedPos, 0.2f);

        foreach (var hit in hits)
        {
            if (hit != null &&
                hit.gameObject != gameObject &&
                hit.CompareTag("Obstacle"))
            {
                // Tile occupied
                transform.position = startPos;
                return;
            }
        }

        //  valid drop
        transform.position = new Vector3(
            snappedPos.x,
            yOffset,
            snappedPos.z
        );
    }
}
