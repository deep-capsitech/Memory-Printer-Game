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
    [Header("Reward Icons")]
    public Sprite snapshotSprite;
    public Sprite coinSprite;
    public Sprite invisionSprite;
    public Sprite freezeSprite;
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
            DailyRewardController.DailyReward reward =
     DailyRewardController.Instance.GetRewardForDay(dayNumber);

            // ---------- REWARD TYPE ----------
            Transform rewardType = dayItems[i].transform.Find("RewardTypeText");
            if (rewardType != null)
            {
                var txt = rewardType.GetComponent<TextMeshProUGUI>();
                if (txt != null)
                    txt.text = reward.type.ToString().ToUpper();
            }

            // ---------- REWARD VALUE ----------
            Transform rewardValue = dayItems[i].transform.Find("RewardValueText");
            if (rewardValue != null)
            {
                var txt = rewardValue.GetComponent<TextMeshProUGUI>();
                if (txt != null)
                {
                    if (reward.type == DailyRewardController.DailyRewardType.Coins)
                        txt.text = reward.amount.ToString();
                    else
                        txt.text = "×" + reward.amount;
                }
            }
            // ---------- REWARD ICON ----------
            Transform rewardIcon = dayItems[i].transform.Find("RewardIcon");
            if (rewardIcon != null)
            {
                Image iconImg = rewardIcon.GetComponent<Image>();
                if (iconImg != null)
                {
                    switch (reward.type)
                    {
                        case DailyRewardController.DailyRewardType.Snapshot:
                            iconImg.sprite = snapshotSprite;
                            break;

                        case DailyRewardController.DailyRewardType.Coins:
                            iconImg.sprite = coinSprite;
                            break;

                        case DailyRewardController.DailyRewardType.Invision:
                            iconImg.sprite = invisionSprite;
                            break;

                        case DailyRewardController.DailyRewardType.Freeze:
                            iconImg.sprite = freezeSprite;
                            break;
                    }
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
        DailyRewardController.DailyReward todayReward =
    DailyRewardController.Instance.GetRewardForDay(displayDay);

        string rewardText;

        if (todayReward.type == DailyRewardController.DailyRewardType.Coins)
            rewardText = todayReward.amount.ToString() + " COINS";
        else
            rewardText = "×" + todayReward.amount + " " + todayReward.type.ToString().ToUpper();

        todayRewardText.text = "TODAY'S REWARD: " + rewardText;

        // Show bonus on Day 7
        if (displayDay == 7)
        {
            todayRewardText.text += " + 200 BONUS COINS";
        }
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

        // Give extra reward manually (same as today)
        int day = DailyRewardController.Instance.GetCurrentDay() - 1;
        if (day <= 0)
            day = 7;

        var reward = DailyRewardController.Instance.GetRewardForDay(day);
        DailyRewardController.Instance.GiveExtraReward(reward);

        RefreshUI();
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