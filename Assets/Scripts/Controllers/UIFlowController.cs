using TMPro;
using UnityEngine;

public class UIFlowController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject gameplayPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;
    public GameObject worldPanel;
    public GameObject levelPanel;
    public GameObject noBatteryPanel;

    public GameObject newWorldPanel;
    public TextMeshProUGUI newWorldNameText;
    public TextMeshProUGUI newWorldQuestionText;

    public GameObject backgroundPanel;

    [Header("HUD")]
    public HUDVisibilityController hud;

    private GameObject _previousPanelBeforeNoBattery;
    
    public DailyRewardController dailyRewardController;

    public void DisableAllPanels()
    {
        menuPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        levelCompletePanel.SetActive(false);
        worldPanel.SetActive(false);

        if (levelPanel != null)
            levelPanel.SetActive(false);

        if (noBatteryPanel != null)
            noBatteryPanel.SetActive(false);

        if (newWorldPanel != null)
            newWorldPanel.SetActive(false);

        backgroundPanel.SetActive(true);
    }

    public void UpdateHUD(HUDVisibilityController.UIState state)
    {
        if (hud != null)
            hud.UpdateHUD(state);
    }

    public void ShowMenu()
    {
        DisableAllPanels();
        menuPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.Menu);
        dailyRewardController.UpdateDailyRewardButton();
        
    }

    public void ShowGameOver()
    {
        DisableAllPanels();
        gameOverPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.GameOver);
    }

    public void PauseGame()
    {
        DisableAllPanels();
        pausePanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.Pause);
    }

    public void ResumeGame()
    {
        DisableAllPanels();
        backgroundPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.Gameplay);
    }

    public void OpenWorldLevels()
    {
        DisableAllPanels();
        worldPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.World);
    }

    public void OpenLevelPanel()
    {
        DisableAllPanels();
        levelPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.Level);
    }
    public void ShowNoBatteryPanel()
    {
        DisableAllPanels();
        noBatteryPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.Menu);
    }

    public void ReturnFromNoBatteryPanel()
    {
        DisableAllPanels();

        if (_previousPanelBeforeNoBattery != null)
            _previousPanelBeforeNoBattery.SetActive(true);
        else
            menuPanel.SetActive(true);
    }
    public void ShowDailyRewardPanel()
    {
        DisableAllPanels();
        dailyRewardController.dailyRewardPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.Menu);
    }
    public void ShowGameplay()
    {
        DisableAllPanels();
        backgroundPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.Gameplay);
    }
    public void ShowWorldSelect()
    {
        DisableAllPanels();
        worldPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.World);
    }
    public void ShowLevelSelect()
    {
        DisableAllPanels();
        levelPanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.Level);
    }
    public void ShowNewWorldPanel(WorldData world)
    {
        DisableAllPanels();
        newWorldPanel.SetActive(true);

        newWorldNameText.text = world.worldName.ToUpper();
        newWorldQuestionText.text = "Do you want to go to this world now?";
    }
    public void ShowLevelComplete()
    {
        DisableAllPanels();
        levelCompletePanel.SetActive(true);
        UpdateHUD(HUDVisibilityController.UIState.LevelComplete);
    }
    public void ShowPowerUpMode()
    {
        DisableAllPanels();
        backgroundPanel.SetActive(false);
    }
}