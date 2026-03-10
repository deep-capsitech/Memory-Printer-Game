using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuyPowerupPanelController : MonoBehaviour
{
    [Header("UI")]
    public Image powerupIcon;
    public TextMeshProUGUI costText;

    [Header("Buttons")]
    public Button buyWithCoinsButton;
    public GameObject watchAdButton;

    [Header("Icons")]
    public Sprite snapshotSprite;
    public Sprite invisionSprite;
    public Sprite freezeSprite;

    private PowerupType currentType;
    private int cost = 100;

    public void Setup(PowerupType type)
    {
        currentType = type;

        switch (type)
        {
            case PowerupType.Snapshot:
                powerupIcon.sprite = snapshotSprite;

                buyWithCoinsButton.gameObject.SetActive(false); // No coins for snapshot
                break;

            case PowerupType.Invision:
                powerupIcon.sprite = invisionSprite;

                buyWithCoinsButton.gameObject.SetActive(true);
                RefreshCoinButtonState();
                break;

            case PowerupType.Freeze:
                powerupIcon.sprite = freezeSprite;

                buyWithCoinsButton.gameObject.SetActive(true);
                RefreshCoinButtonState();
                break;
        }

        costText.text = cost.ToString();
    }

    void RefreshCoinButtonState()
    {
        int currentCoins = GameEconomyManager.Instance.GetCoins();
        buyWithCoinsButton.interactable = currentCoins >= cost;
    }

    public void OnBuyWithCoins()
    {
        bool success = false;

        if (currentType == PowerupType.Invision)
            success = PowerupInventoryManager.Instance.BuyInvision(cost);
        else if (currentType == PowerupType.Freeze)
            success = PowerupInventoryManager.Instance.BuyFreeze(cost);

        if (success)
        {
            Close();
            GameManagerCycle.Instance.powerUpController.UpdatePowerUpUI();
        }
    }
    public void OnWatchAd()
    {
        AdManager.Instance.ShowRewarded(() =>
        {
            switch (currentType)
            {
                case PowerupType.Snapshot:
                    GameManagerCycle.Instance.AddSnapshotUse();
                    break;

                case PowerupType.Invision:
                    PowerupInventoryManager.Instance.RefillViaAdInvision();
                    break;

                case PowerupType.Freeze:
                    PowerupInventoryManager.Instance.RefillViaAdFreeze();
                    break;
            }

            Close();
            GameManagerCycle.Instance.powerUpController.UpdatePowerUpUI();
        });
    }

    public void OnClose()
    {
        Close();
    }

    void Close()
    {
        gameObject.SetActive(false);
        GameManagerCycle.Instance.uiFlowController.ShowGameplay();
    }
}