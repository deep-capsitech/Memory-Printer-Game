using UnityEngine;

public class BackButtonManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject worldSelectPanel;
    public GameObject levelSelectPanel;
    public GameObject gameplayPanel;
    public GameObject pausePanel;

    [Header("Exit Popup")]
    public GameObject exitPopup;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBack();
        }
    }

    void HandleBack()
    {
        var gm = GameManagerCycle.Instance;

        // 🔴 MENU → EXIT
        if (menuPanel.activeInHierarchy)
        {
            if (!exitPopup.activeSelf)
                ShowExitPopup();
        }

        // 🔙 WORLD SELECT → MENU
        else if (worldSelectPanel.activeInHierarchy)
        {
            gm.uiFlowController.ShowMenu();
        }

        // 🔙 LEVEL SELECT → WORLD SELECT
        else if (levelSelectPanel.activeInHierarchy)
        {
            gm.uiFlowController.ShowWorldSelect();
        }

        // 🎮 GAMEPLAY → PAUSE
        else if (gameplayPanel.activeInHierarchy)
        {
            gm.PauseGame();
        }

        // ⏸ PAUSE → RESUME
        else if (pausePanel.activeInHierarchy)
        {
            gm.ResumeGame();
        }
    }

    void ShowExitPopup()
    {
        exitPopup.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnClick_No()
    {
        exitPopup.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnClick_Yes()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}