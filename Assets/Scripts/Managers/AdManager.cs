using GoogleMobileAds.Api;
using System;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;

#if UNITY_ANDROID
    public string bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
    public string interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
    public string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IOS
    public string bannerAdUnitId = "ca-app-pub-3940256099942544/2934735716";
    public string interstitialAdUnitId = "ca-app-pub-3940256099942544/4411468910";
    public string rewardedAdUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
    public string bannerAdUnitId = "unused";
    public string interstitialAdUnitId = "unused";
    public string rewardedAdUnitId = "unused";
#endif

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            LoadBannerAd();
            LoadInterstitialAd();
            LoadRewardedAd();

            ShowBanner();
        });
    }

    #region Banner

    public void LoadBannerAd()
    {
        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);
    }

    public void ShowBanner()
    {
        if (bannerView != null)
            bannerView.Show();
    }

    public void HideBanner()
    {
        if (bannerView != null)
            bannerView.Hide();
    }

    #endregion

    #region Interstitial

    public void LoadInterstitialAd()
    {
        InterstitialAd.Load(interstitialAdUnitId, new AdRequest(),
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.Log("Interstitial failed. Retrying...");
                    Invoke(nameof(LoadInterstitialAd), 3f);
                    return;
                }

                interstitialAd = ad;
            });
    }

    public void ShowInterstitial()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
            LoadInterstitialAd(); // preload next
        }
        else
        {
            Debug.Log("Interstitial not ready");
        }
    }

    #endregion

    #region Rewarded

    public void LoadRewardedAd()
    {
        RewardedAd.Load(rewardedAdUnitId, new AdRequest(),
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.Log("Rewarded failed. Retrying...");
                    Invoke(nameof(LoadRewardedAd), 3f);
                    return;
                }
                rewardedAd = ad;
            });
    }

    public void ShowRewarded(Action onRewardEarned = null)
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log("Reward Earned: " + reward.Amount);
                onRewardEarned?.Invoke();
            });

            LoadRewardedAd(); // preload next
        }
        else
        {
            Debug.Log("Rewarded not ready");
        }
    }

    #endregion
}
