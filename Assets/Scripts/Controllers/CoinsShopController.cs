using UnityEngine;
using TMPro;

public class CoinsShopController : MonoBehaviour
{
    public int rewardCoins = 50;
    public TextMeshProUGUI coinsText;

    void OnEnable()
    {
        UpdateCoinsUI();
    }

    public void WatchAdForCoins()
    {
        AdManager.Instance.ShowRewarded(() =>
        {
            GameEconomyManager.Instance.AddCoins(rewardCoins);
            UpdateCoinsUI();
        });
    }

    void UpdateCoinsUI()
    {
        int coins = GameEconomyManager.Instance.GetCoins();
        coinsText.text = coins.ToString();
    }
}