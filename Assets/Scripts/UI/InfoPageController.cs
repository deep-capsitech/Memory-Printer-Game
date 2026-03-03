using UnityEngine;

public class InfoPageController : MonoBehaviour
{
    public GameObject pageObjective;
    public GameObject pagePowerups;

    private int currentPage = 0;

    void Start()
    {
        ShowPage(0);
    }

    public void Next()
    {
        currentPage++;
        if (currentPage > 1)
            currentPage = 1;

        ShowPage(currentPage);
    }

    public void Previous()
    {
        currentPage--;
        if (currentPage < 0)
            currentPage = 0;

        ShowPage(currentPage);
    }

    void ShowPage(int index)
    {
        pageObjective.SetActive(index == 0);
        pagePowerups.SetActive(index == 1);
    }
}