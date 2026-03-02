using UnityEngine;
using UnityEngine.UI;

public class HorizontalSnap : MonoBehaviour
{
    public ScrollRect scrollRect;
    public int totalPages = 2;
    private int currentPage = 0;

    public void Next()
    {
        currentPage++;
        if (currentPage >= totalPages)
            currentPage = totalPages - 1;

        SnapToPage();
    }

    public void Previous()
    {
        currentPage--;
        if (currentPage < 0)
            currentPage = 0;

        SnapToPage();
    }

    void SnapToPage()
    {
        float target = (float)currentPage / (totalPages - 1);
        scrollRect.horizontalNormalizedPosition = target;
    }
}