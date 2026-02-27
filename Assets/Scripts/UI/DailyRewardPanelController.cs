using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardPanelController : MonoBehaviour
{
    [Header("Day Items")]
    public GameObject[] dayItems; // Size = 7

    [Header("Texts")]
    public TextMeshProUGUI todayRewardText;

    [Header("Buttons")]
    public Button collectButton;
    public Button watchAdButton;
    public Button closeButton;

    void OnEnable()
    {
        RefreshUI();

        collectButton.onClick.RemoveAllListeners();
        watchAdButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();

        collectButton.onClick.AddListener(OnCollectClicked);
        watchAdButton.onClick.AddListener(OnWatchAdClicked);
        closeButton.onClick.AddListener(OnCloseClicked);
    }
    void RefreshUI()
    {
        if (DailyRewardController.Instance == null)
            return;

        int rawDay = DailyRewardController.Instance.GetCurrentDay();
        bool claimedToday = DailyRewardController.Instance.HasClaimedTodayPublic();

        for (int i = 0; i < dayItems.Length; i++)
        {
            if (dayItems[i] == null)
                continue;

            int dayNumber = i + 1;

            // ---------- DAY LABEL ----------
            Transform dayLabel = dayItems[i].transform.Find("DayLabel");
            if (dayLabel != null)
            {
                var txt = dayLabel.GetComponent<TextMeshProUGUI>();
                if (txt != null)
                    txt.text = "DAY " + dayNumber;
            }

            // ---------- REWARD TYPE ----------
            Transform rewardType = dayItems[i].transform.Find("RewardTypeText");
            if (rewardType != null)
            {
                var txt = rewardType.GetComponent<TextMeshProUGUI>();
                if (txt != null)
                    txt.text = DailyRewardController.Instance.GetRewardName(dayNumber);
            }

            // ---------- REWARD VALUE ----------
            Transform rewardValue = dayItems[i].transform.Find("RewardValueText");
            if (rewardValue != null)
            {
                var txt = rewardValue.GetComponent<TextMeshProUGUI>();
                if (txt != null)
                {
                    int amount = DailyRewardController.Instance.GetRewardAmount(dayNumber);
                    bool isCoins = DailyRewardController.Instance.GetRewardName(dayNumber) == "COINS";

                    txt.text = isCoins ? amount.ToString() : "×" + amount;
                }
            }

            // ---------- CLAIMED CHECK ----------
            Transform claimedCheck = dayItems[i].transform.Find("ClaimedCheck");
            if (claimedCheck != null)
            {
                claimedCheck.gameObject.SetActive(dayNumber < rawDay);
            }

            // ---------- BACKGROUND COLORS ----------
            Image bg = dayItems[i].GetComponent<Image>();
            if (bg != null)
            {
                if (!claimedToday && dayNumber == rawDay)
                {
                    // Highlight ONLY if not claimed
                    bg.color = new Color(0.1f, 1f, 0.6f, 1f);
                }
                else if (dayNumber < rawDay)
                {
                    // Claimed days dim
                    bg.color = new Color(1f, 1f, 1f, 0.6f);
                }
                else
                {
                    // Future days
                    bg.color = new Color(1f, 1f, 1f, 0.85f);
                }
            }
        }

        // ---------- BUTTON STATES ----------
        collectButton.interactable = !claimedToday;
        watchAdButton.interactable = !claimedToday;

        if (claimedToday)
            collectButton.GetComponentInChildren<TextMeshProUGUI>().text = "CLAIMED";
        else
            collectButton.GetComponentInChildren<TextMeshProUGUI>().text = "COLLECT";
        // ---------- TODAY REWARD TEXT ----------
        int displayDay;

        // If already claimed, show the reward that was just claimed
        if (claimedToday)
        {
            displayDay = rawDay - 1;
            if (displayDay < 1)
                displayDay = 7;
        }
        else
        {
            displayDay = rawDay;
        }
        int rewardAmount = DailyRewardController.Instance.GetRewardAmount(displayDay);
        string rewardName = DailyRewardController.Instance.GetRewardName(displayDay);
        bool rewardIsCoins = rewardName == "COINS";

        todayRewardText.text =
            "TODAY'S REWARD: " +
            (rewardIsCoins ? rewardAmount.ToString() : "×" + rewardAmount) +
            " " + rewardName;
    }

    void OnCollectClicked()
    {
        DailyRewardController.Instance.ClaimReward();
        RefreshUI();
        ClosePanel();
    }

    void OnWatchAdClicked()
    {
        // Simulated ad success
        DailyRewardController.Instance.ClaimReward();
        DailyRewardController.Instance.ClaimReward(); // double reward
        RefreshUI() ;
        ClosePanel();
    }

    void OnCloseClicked()
    {
        ClosePanel();
    }

    void ClosePanel()
    {
        gameObject.SetActive(false);
        GameManagerCycle.Instance.uiFlowController.ShowMenu();
    }
}