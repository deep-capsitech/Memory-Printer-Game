using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PurchaseType
{
    Battery,
    Invision,
    Freeze
}
public class PurchasePanelController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI costText;
    public TextMeshProUGUI timerText;

    [Header("Sections")]
    public GameObject batterySection;
    public GameObject powerupSection;

    [Header("Title")]
    public TextMeshProUGUI titleText;

    [Header("Powerup Icon")]
    public Image powerupIcon;

    [Header("Buttons")]
    public Button buyButton;
    public Button watchAdButton;

    [Header("Icons")]
    public Sprite invisionSprite;
    public Sprite freezeSprite;

    private PurchaseType currentType;
    private int cost = 100;

    void Update()
    {
        if (currentType == PurchaseType.Battery)
        {
            UpdateBatteryTimer();
        }
        RefreshCoinState();
    }

    public void Setup(PurchaseType type)
    {
        currentType = type;

        batterySection.SetActive(false);
        powerupSection.SetActive(false);

        switch (type)
        {
            case PurchaseType.Battery:

                titleText.text = "NO BATTERY";

                batterySection.SetActive(true);

                break;

            case PurchaseType.Invision:

                titleText.text = "NO POWERUP";

                powerupSection.SetActive(true);
                powerupIcon.sprite = invisionSprite;

                break;

            case PurchaseType.Freeze:

                titleText.text = "NO POWERUP";

                powerupSection.SetActive(true);
                powerupIcon.sprite = freezeSprite;

                break;
        }
        costText.text = cost.ToString();
        RefreshCoinState();
    }

    void RefreshCoinState()
    {
        int coins = GameEconomyManager.Instance.GetCoins();
        buyButton.interactable = coins >= cost;
    }

    void UpdateBatteryTimer()
    {
        float seconds = BatteryManager.Instance.GetSecondsUntilNextBattery();

        int minutes = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);

        timerText.text = $"{minutes:00}:{secs:00}";

        if (BatteryManager.Instance.HasBattery())
        {
            Close();
        }
    }

    public void OnBuy()
    {
        bool success = false;

        switch (currentType)
        {
            case PurchaseType.Battery:

                if (GameEconomyManager.Instance.GetCoins() < cost)
                    return;

                GameEconomyManager.Instance.SpendCoins(cost);
                BatteryManager.Instance.AddBatteryInstant(1);
                success = true;

                break;

            case PurchaseType.Invision:

                success = PowerupInventoryManager.Instance.BuyInvision(cost);
                break;

            case PurchaseType.Freeze:

                success = PowerupInventoryManager.Instance.BuyFreeze(cost);
                break;
        }

        if (success)
        {
            GameManagerCycle.Instance.powerUpController.UpdatePowerUpUI();
            Close();
        }
    }

    public void OnWatchAd()
    {
        AdManager.Instance.ShowRewarded(() =>
        {
            switch (currentType)
            {
                case PurchaseType.Battery:
                    BatteryManager.Instance.AddBatteryInstant(1);
                    break;

                case PurchaseType.Invision:
                    PowerupInventoryManager.Instance.RefillViaAdInvision();
                    break;

                case PurchaseType.Freeze:
                    PowerupInventoryManager.Instance.RefillViaAdFreeze();
                    break;
            }

            GameManagerCycle.Instance.powerUpController.UpdatePowerUpUI();
            Close();
        });
    }

    public void Close()
    {
        gameObject.SetActive(false);
        GameManagerCycle.Instance.uiFlowController.ShowGameplay();
    }
}