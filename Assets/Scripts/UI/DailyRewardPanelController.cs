using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        if (DailyRewardManager.Instance == null)
            return;

        int rawDay = DailyRewardManager.Instance.GetCurrentDay();
        bool claimedToday = DailyRewardManager.Instance.HasClaimedTodayPublic();

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
                    txt.text = GetRewardName(dayNumber);
            }

            // ---------- REWARD VALUE ----------
            Transform rewardValue = dayItems[i].transform.Find("RewardValueText");
            if (rewardValue != null)
            {
                var txt = rewardValue.GetComponent<TextMeshProUGUI>();
                if (txt != null)
                {
                    int amount = GetRewardAmount(dayNumber);
                    bool isCoins = GetRewardName(dayNumber) == "COINS";

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
    }

    int GetRewardAmount(int day)
    {
        switch (day)
        {
            case 1: return 50;
            case 2: return 1;
            case 3: return 75;
            case 4: return 1;
            case 5: return 2;
            case 6: return 100;
            case 7: return 200;
            default: return 0;
        }
    }

    string GetRewardName(int day)
    {
        switch (day)
        {
            case 1: return "COINS";
            case 2: return "BATTERY";
            case 3: return "COINS";
            case 4: return "FREEZE";
            case 5: return "BATTERIES";
            case 6: return "COINS";
            case 7: return "COINS";
            default: return "";
        }
    }

    void OnCollectClicked()
    {
        DailyRewardManager.Instance.ClaimReward();
        ClosePanel();
    }

    void OnWatchAdClicked()
    {
        // Simulated ad success
        DailyRewardManager.Instance.ClaimReward();
        DailyRewardManager.Instance.ClaimReward(); // double reward
        ClosePanel();
    }

    void OnCloseClicked()
    {
        ClosePanel();
    }

    void ClosePanel()
    {
        gameObject.SetActive(false);
        GameManagerCycle.Instance.ShowMenu();
    }
}