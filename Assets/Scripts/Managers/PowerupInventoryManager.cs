using UnityEngine;

public class PowerupInventoryManager : MonoBehaviour
{
    public static PowerupInventoryManager Instance;

    private const string INVISION_KEY = "INVISION_COUNT";
    private const string FREEZE_KEY = "FREEZE_COUNT";
    private const string POWERUP_INIT_KEY = "POWERUP_INITIALIZED";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeInventory();
    }

    void InitializeInventory()
    {
        if (!PlayerPrefs.HasKey(POWERUP_INIT_KEY))
        {
            PlayerPrefs.SetInt(INVISION_KEY, 3);
            PlayerPrefs.SetInt(FREEZE_KEY, 3);
            PlayerPrefs.SetInt(POWERUP_INIT_KEY, 1);
            PlayerPrefs.Save();
        }
    }

    // ---------------------------
    // GETTERS
    // ---------------------------

    public int GetInvisionCount()
    {
        return PlayerPrefs.GetInt(INVISION_KEY, 0);
    }

    public int GetFreezeCount()
    {
        return PlayerPrefs.GetInt(FREEZE_KEY, 0);
    }

    // ---------------------------
    // ADD METHODS (Daily Reward)
    // ---------------------------

    public void AddInvision(int amount)
    {
        int count = GetInvisionCount();
        PlayerPrefs.SetInt(INVISION_KEY, count + amount);
        PlayerPrefs.Save();
    }

    public void AddFreeze(int amount)
    {
        int count = GetFreezeCount();
        PlayerPrefs.SetInt(FREEZE_KEY, count + amount);
        PlayerPrefs.Save();
    }

    // ---------------------------
    // CONSUME METHODS
    // ---------------------------

    public bool ConsumeInvision()
    {
        int count = GetInvisionCount();
        if (count <= 0)
            return false;

        PlayerPrefs.SetInt(INVISION_KEY, count - 1);
        PlayerPrefs.Save();
        return true;
    }

    public bool ConsumeFreeze()
    {
        int count = GetFreezeCount();
        if (count <= 0)
            return false;

        PlayerPrefs.SetInt(FREEZE_KEY, count - 1);
        PlayerPrefs.Save();
        return true;
    }

    // ---------------------------
    // PURCHASE (Coins)
    // ---------------------------

    public bool BuyInvision(int cost)
    {
        if (!GameEconomyManager.Instance.SpendCoins(cost))
            return false;

        AddInvision(1);
        return true;
    }

    public bool BuyFreeze(int cost)
    {
        if (!GameEconomyManager.Instance.SpendCoins(cost))
            return false;

        AddFreeze(1);
        return true;
    }

    // ---------------------------
    // AD REFILL (Hook only)
    // ---------------------------

    public void RefillViaAdInvision()
    {
        AddInvision(1);
    }

    public void RefillViaAdFreeze()
    {
        AddFreeze(1);
    }
}