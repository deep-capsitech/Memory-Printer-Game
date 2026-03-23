using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CoinsManager : MonoBehaviour
{
    string L(string key, params object[] args)
    {
        return LocalizationManager.Instance.GetText(key, args);
    }
    public int rewardCoins = 100;

    public TMP_Text buttonText;

    public Image cardImage;
    public Sprite normalCard;
    public Sprite claimedCard;

    public GameObject adIcon;   // Ad icon to hide after claim

    private const string CLAIM_DATE_KEY = "COINS_CLAIM_DATE";

    void OnEnable()
    {
        CheckClaimState();
    }

    void CheckClaimState()
    {
        string lastClaim = PlayerPrefs.GetString(CLAIM_DATE_KEY, "");
        string today = DateTime.Now.ToString("yyyyMMdd");

        if (lastClaim == today)
        {
            SetClaimedState();
        }
        else
        {
            SetAvailableState();
        }
    }

    public void ClaimCoins()
    {
        string lastClaim = PlayerPrefs.GetString(CLAIM_DATE_KEY, "");
        string today = DateTime.Now.ToString("yyyyMMdd");

        if (lastClaim == today)
            return;

        AdManager.Instance.ShowRewarded(() =>
        {
            GameEconomyManager.Instance.AddCoins(rewardCoins);

            PlayerPrefs.SetString(CLAIM_DATE_KEY, today);
            PlayerPrefs.Save();

            SetClaimedState();
        });
    }

    void SetClaimedState()
    {
        buttonText.text = L("CLAIMED");
        cardImage.sprite = claimedCard;

        if (adIcon != null)
            adIcon.SetActive(false);
    }

    void SetAvailableState()
    {
        buttonText.text = L("CLAIM");
        cardImage.sprite = normalCard;

        if (adIcon != null)
            adIcon.SetActive(true);
    }
}