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
    public Button closeButton;
    [Header("Reward Icons")]
    public Sprite snapshotSprite;
    public Sprite coinSprite;
    public Sprite invisionSprite;
    public Sprite freezeSprite;
    public Sprite mysteryBoxSprite;

    public UIFlowController uiFlowController;
    public GameObject claimPanel;
    public Image claimRewardIcon;
    public Button claimAdButton;
    // public Button claimCloseButton;

    [Header("Card Sprites")]
    public Sprite normalCardSprite;
    public Sprite claimedCardSprite;
    public Material normalMaterial;
    public Material outlineMaterial;
    void OnEnable()
    {
        RefreshUI();

        collectButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
        collectButton.onClick.AddListener(OnCollectClicked);
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
                {
                    if (dayNumber == 7)
                        txt.text = "MYSTERY BOX";
                    else
                        txt.text = reward.type.ToString().ToUpper();
                }
            }
            // ---------- REWARD VALUE ----------
            Transform rewardValue = dayItems[i].transform.Find("RewardValueText");
            if (rewardValue != null)
            {
                var txt = rewardValue.GetComponent<TextMeshProUGUI>();
                if (txt != null)
                {
                    if (dayNumber == 7)
                    {
                        txt.text = "×1"; // Mystery Box count
                    }
                    else if (reward.type == DailyRewardController.DailyRewardType.Coins)
                    {
                        txt.text = reward.amount.ToString();
                    }
                    else
                    {
                        txt.text = "×" + reward.amount;
                    }
                }
            }
            // ---------- REWARD ICON ----------
            Transform rewardIcon = dayItems[i].transform.Find("RewardIcon");
            if (rewardIcon != null)
            {
                Image iconImg = rewardIcon.GetComponent<Image>();
                if (dayNumber == 7)
                {
                    iconImg.sprite = mysteryBoxSprite;
                }
                else
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

            Image bg = dayItems[i].GetComponent<Image>();
            if (bg != null)
            {
                if (dayNumber < rawDay)
                {
                    bg.sprite = claimedCardSprite; // green card
                }
                else
                {
                    bg.sprite = normalCardSprite; // normal card
                }
            }
        }
        var btnText = collectButton.GetComponentInChildren<TextMeshProUGUI>();

        collectButton.interactable = true;

        if (claimedToday)
        {
            btnText.text = "CLAIMED";
            btnText.fontMaterial = outlineMaterial;
        }
        else
        {
            btnText.text = "CLAIM";
            btnText.fontMaterial = normalMaterial;
        }
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

        if (displayDay == 7)
        {
            todayRewardText.text = "TODAY'S REWARD: MYSTERY BOX";
        }
    }
    void OnCollectClicked()
    {
        // 🚫 If already claimed → do nothing
        if (DailyRewardController.Instance.HasClaimedTodayPublic())
            return;

        int day = DailyRewardController.Instance.GetCurrentDay();
        var reward = DailyRewardController.Instance.GetRewardForDay(day);

        DailyRewardController.Instance.ClaimReward();

        ShowClaimPanel(reward);

        RefreshUI();
    }

    void ShowClaimPanel(DailyRewardController.DailyReward reward)
    {
        uiFlowController.ShowClaimRewardPanel();

        switch (reward.type)
        {
            case DailyRewardController.DailyRewardType.Snapshot:
                claimRewardIcon.sprite = snapshotSprite;
                break;

            case DailyRewardController.DailyRewardType.Coins:
                claimRewardIcon.sprite = coinSprite;
                break;

            case DailyRewardController.DailyRewardType.Invision:
                claimRewardIcon.sprite = invisionSprite;
                break;

            case DailyRewardController.DailyRewardType.Freeze:
                claimRewardIcon.sprite = freezeSprite;
                break;
        }

        claimAdButton.onClick.RemoveAllListeners();
        claimAdButton.onClick.AddListener(() => WatchAdDouble(reward));

        // 🔥 Auto close after 2 seconds
        CancelInvoke(nameof(CloseClaimPanel));
        Invoke(nameof(CloseClaimPanel), 2f);
    }

    void WatchAdDouble(DailyRewardController.DailyReward reward)
    {
        AdManager.Instance.ShowRewarded(() =>
        {
            DailyRewardController.Instance.GiveExtraReward(reward);

            CloseClaimPanel();
        });
    }
    public void CloseClaimPanel()
    {
        CancelInvoke(nameof(CloseClaimPanel)); // prevent double call
        GameManagerCycle.Instance.uiFlowController.ShowDailyRewardPanel();
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