using System;
using UnityEngine;
#if MRU_GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
#endif
#if MRU_GOOGLE_MOBILE_ADS && MRU_UMP
using GoogleMobileAds.Ump.Api;
#endif

namespace MRU.InfiniteRunner
{
    public sealed class AdService : MonoBehaviour
    {
        private const string TestInterstitialAndroid = "ca-app-pub-3940256099942544/1033173712";
        private const string TestRewardedAndroid = "ca-app-pub-3940256099942544/5224354917";
        private const string InterstitialRunKey = "mru_ad_run_count";
        private const string LastInterstitialKey = "mru_ad_last_utc";

        private AdConfiguration configuration;

#if MRU_GOOGLE_MOBILE_ADS
        private InterstitialAd interstitial;
        private RewardedAd rewarded;
        private Action<bool> rewardCompletion;
        private bool rewardEarned;
#endif
        private bool initialized;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            configuration = Resources.Load<AdConfiguration>("MRUAdConfiguration");
            if (configuration == null)
                Debug.LogWarning("MRU ad configuration is missing; ads will use test IDs when enabled.");
            InitializeWithConsent();
        }

        private void InitializeWithConsent()
        {
#if MRU_GOOGLE_MOBILE_ADS && MRU_UMP
            ConsentInformation.Update(new ConsentRequestParameters(), error =>
            {
                if (error != null) Debug.LogWarning("UMP consent update failed: " + error.Message);
                ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
                {
                    if (formError != null) Debug.LogWarning("UMP consent form: " + formError.Message);
                    if (ConsentInformation.CanRequestAds()) InitializeAds();
                });
            });
#elif MRU_GOOGLE_MOBILE_ADS
            InitializeAds();
#else
            Debug.Log("Mobile ads are disabled. Import the official Google Mobile Ads Unity plugin and configure it before release.");
#endif
        }

#if MRU_GOOGLE_MOBILE_ADS
        private void InitializeAds()
        {
            if (initialized) return;
            initialized = true;
            MobileAds.Initialize(_ =>
            {
                LoadInterstitial();
                LoadRewarded();
            });
        }

        private string InterstitialId
        {
            get
            {
                if (configuration == null || configuration.useTestAdUnits) return TestInterstitialAndroid;
                return configuration.productionInterstitialAndroid;
            }
        }

        private string RewardedId
        {
            get
            {
                if (configuration == null || configuration.useTestAdUnits) return TestRewardedAndroid;
                return configuration.productionRewardedAndroid;
            }
        }

        private void LoadInterstitial()
        {
            if (string.IsNullOrEmpty(InterstitialId)) return;
            InterstitialAd.Load(InterstitialId, new AdRequest(), (ad, error) =>
            {
                if (error != null) { Debug.LogWarning("Interstitial load failed: " + error.GetMessage()); return; }
                interstitial = ad;
                interstitial.OnAdFullScreenContentClosed += () => { interstitial.Destroy(); interstitial = null; LoadInterstitial(); };
                interstitial.OnAdFullScreenContentFailed += _ => { interstitial.Destroy(); interstitial = null; LoadInterstitial(); };
            });
        }

        private void LoadRewarded()
        {
            if (string.IsNullOrEmpty(RewardedId)) return;
            RewardedAd.Load(RewardedId, new AdRequest(), (ad, error) =>
            {
                if (error != null) { Debug.LogWarning("Rewarded load failed: " + error.GetMessage()); return; }
                rewarded = ad;
                rewarded.OnAdFullScreenContentClosed += () =>
                {
                    CompleteReward();
                    ad.Destroy();
                    if (rewarded == ad) rewarded = null;
                    LoadRewarded();
                };
                rewarded.OnAdFullScreenContentFailed += _ =>
                {
                    rewardEarned = false;
                    CompleteReward();
                    ad.Destroy();
                    if (rewarded == ad) rewarded = null;
                    LoadRewarded();
                };
            });
        }

        private void CompleteReward()
        {
            Action<bool> callback = rewardCompletion;
            rewardCompletion = null;
            if (callback != null) callback(rewardEarned);
        }
#endif

        public void ShowRewarded(Action<bool> completed)
        {
#if MRU_GOOGLE_MOBILE_ADS
            if (rewarded == null || !rewarded.CanShowAd())
            {
                completed(false);
                return;
            }
            rewardEarned = false;
            rewardCompletion = completed;
            rewarded.Show(_ => rewardEarned = true);
#else
            completed(false);
#endif
        }

        public void TryShowInterstitialAtResults()
        {
#if MRU_GOOGLE_MOBILE_ADS
            int runCount = PlayerPrefs.GetInt(InterstitialRunKey, 0) + 1;
            PlayerPrefs.SetInt(InterstitialRunKey, runCount);
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long last = long.TryParse(PlayerPrefs.GetString(LastInterstitialKey, "0"), out long saved) ? saved : 0;
            if (runCount % 3 != 0 || now - last < 180 || interstitial == null || !interstitial.CanShowAd()) return;
            PlayerPrefs.SetString(LastInterstitialKey, now.ToString());
            PlayerPrefs.Save();
            interstitial.Show();
#endif
        }

        private void OnDestroy()
        {
#if MRU_GOOGLE_MOBILE_ADS
            if (interstitial != null) interstitial.Destroy();
            if (rewarded != null) rewarded.Destroy();
#endif
        }
    }
}
