using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;

    void Start()
    {
        UpdateCoins();
    }

    void Update()
    {
        // Safe for now, optimized later with events
        UpdateCoins();
    }

    void UpdateCoins()
    {
        if (GameEconomyManager.Instance == null) return;

        coinText.text = GameEconomyManager.Instance.GetCoins().ToString();
    }
}
