using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PurchaseType
{
    Battery,
    Invision,
    Freeze
}

public enum PurchaseSource
{
    None,
    GameOver,
    LevelPanel,
    Gameplay
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
    public PurchaseSource source;

    void Update()
    {
        if (currentType == PurchaseType.Battery)
        {
            UpdateBatteryTimer();
        }
        RefreshCoinState();
    }

    public void Setup(PurchaseType type,PurchaseSource from)
    {
        currentType = type;
        source = from;

        batterySection.SetActive(false);
        powerupSection.SetActive(false);

        switch (type)
        {
            case PurchaseType.Battery:
                titleText.text = LocalizationManager.Instance.GetText("NO_BATTERY");
                batterySection.SetActive(true);
                break;

            case PurchaseType.Invision:
                titleText.text = LocalizationManager.Instance.GetText("NO_POWERUP");
                powerupSection.SetActive(true);
                powerupIcon.sprite = invisionSprite;
                break;

            case PurchaseType.Freeze:
                titleText.text = LocalizationManager.Instance.GetText("NO_POWERUP");
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
            HandlePostPurchaseFlow();
        }
    }

    public void OnWatchAd()
    {
        AdManager.Instance.ShowRewarded(
    onRewardEarned: () =>
    {
        // ONLY reward here
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
    },
    onAdClosed: () =>
    {
        // ONLY navigation here
        HandlePostPurchaseFlow();
    }
);
    }

    public void Close()
    {
        gameObject.SetActive(false);

        switch (source)
        {
            case PurchaseSource.GameOver:
                GameManagerCycle.Instance.uiFlowController.ShowGameOver();
                break;

            case PurchaseSource.LevelPanel:
                GameManagerCycle.Instance.uiFlowController.ShowLevelSelect();
                break;

            case PurchaseSource.Gameplay:
                GameManagerCycle.Instance.uiFlowController.ShowGameplay();
                break;

            default:
                GameManagerCycle.Instance.uiFlowController.ShowMenu();
                break;
        }
    }
    void HandlePostPurchaseFlow()
    {
        gameObject.SetActive(false);

        switch (source)
        {
            case PurchaseSource.LevelPanel:
                // Directly start selected level
                GameManagerCycle.Instance.OnLevelSelected(
                    GameManagerCycle.Instance.CurrentLevelNumber
                );
                break;

            case PurchaseSource.GameOver:
                // Retry level directly
                GameManagerCycle.Instance.Retry();
                break;

            default:
                Close(); // fallback (unchanged behavior)
                break;
        }
    }
    void RefreshUI()
    {
        switch (currentType)
        {
            case PurchaseType.Battery:
                titleText.text = LocalizationManager.Instance.GetText("NO_BATTERY");
                break;

            case PurchaseType.Invision:
            case PurchaseType.Freeze:
                titleText.text = LocalizationManager.Instance.GetText("NO_POWERUP");
                break;
        }
    }
    void OnEnable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += RefreshUI;
            RefreshUI();
        }
    }

    void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= RefreshUI;
    }
}