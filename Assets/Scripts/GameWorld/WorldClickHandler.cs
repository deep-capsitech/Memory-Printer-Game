using UnityEngine;

public class WorldClickHandler : MonoBehaviour
{
    public int worldIndex;

    public void OpenWorld()
    {
        GameManagerCycle.Instance.OpenWorldLevels(worldIndex);
    }
    public void CloseWorld()
    {
        GameManagerCycle.Instance.BackToWorldPanel();
    }
}
