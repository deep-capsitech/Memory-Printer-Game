using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DailyRewardController : MonoBehaviour
{
    [Header("Daily Reward Panel")]
    public GameObject dailyRewardPanel;

    [Header("Daily Reward Button")]
    public Button dailyRewardButton;
    public TextMeshProUGUI dailyRewardButtonText;
    public Image dailyRewardButtonIcon;

    private const string DAILY_POPUP_DATE = "DailyRewardPopupDate";

    public void Initialize()
    {
        UpdateDailyRewardButton();
    }

    public bool ShouldAutoShowDailyReward()
    {
        if (DailyRewardManager.Instance == null)
            return false;

        if (!DailyRewardManager.Instance.CanShowDailyReward())
            return false;

        string lastPopupDate = PlayerPrefs.GetString(DAILY_POPUP_DATE, "");
        string today = System.DateTime.UtcNow.ToString("yyyyMMdd");

        if (lastPopupDate == today)
            return false;

        PlayerPrefs.SetString(DAILY_POPUP_DATE, today);
        PlayerPrefs.Save();

        return true;
    }

    public void UpdateDailyRewardButton()
    {
        if (DailyRewardManager.Instance == null)
            return;

        bool canClaim = DailyRewardManager.Instance.CanShowDailyReward();

        dailyRewardButton.interactable = true; // always clickable

        if (canClaim)
        {
            dailyRewardButtonText.text = "CLAIM";
            dailyRewardButtonIcon.color = Color.white;
        }
        else
        {
            dailyRewardButtonText.text = "CLAIMED";
            dailyRewardButtonIcon.color = Color.white;
        }
    }

    public void OnDailyRewardButtonClicked(UIFlowController uiFlowController)
    {
        uiFlowController.DisableAllPanels();
        dailyRewardPanel.SetActive(true);
        uiFlowController.UpdateHUD(HUDVisibilityController.UIState.Menu);
    }
}