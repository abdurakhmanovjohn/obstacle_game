using System;
using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using Unity.Services.LevelPlay;
#endif

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    [Header("LevelPlay (Android only)")]
    [SerializeField] string appKey = "YOUR_APP_KEY";
    [SerializeField] string rewardedAdUnitId = "YOUR_REWARDED_ID";
    [SerializeField] string interstitialAdUnitId = "YOUR_INTERSTITIAL_ID";
    [SerializeField] string bannerAdUnitId = "YOUR_BANNER_ID";

    [Header("Debug")]
    [Tooltip("Shows the ad status in the top-right corner of the screen. Turn off before release.")]
    [SerializeField] bool showAdStatus = true;

    string initStatus = "init: waiting";
    string rewardedStatus = "rewarded: -";
    string interstitialStatus = "interstitial: -";
    string bannerStatus = "banner: -";
    GUIStyle statusStyle;

    Action onRewarded;
    Action onRewardFailed;
    Action onInterstitialDone;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        PlatformInit();
    }

    void Log(ref string field, string text)
    {
        field = text;
        Debug.Log("[Ads] " + text);
    }

    void OnGUI()
    {
        if (!showAdStatus) return;
        if (statusStyle == null)
        {
            statusStyle = new GUIStyle(GUI.skin.label);
            statusStyle.fontSize = Mathf.Max(14, Screen.height / 45);
            statusStyle.alignment = TextAnchor.UpperRight;
            statusStyle.normal.textColor = Color.yellow;
        }
        string text = initStatus + "\n" + rewardedStatus + "\n" + interstitialStatus + "\n" + bannerStatus;
        GUI.Label(new Rect(Screen.width - 820, 10, 800, 300), text, statusStyle);
    }

    // ===== Public API (the game calls this) =====

    public bool IsRewardedReady()
    {
        return PlatformIsRewardedReady();
    }

    public void ShowRewarded(Action onReward, Action onFail = null)
    {
        if (!PlatformIsRewardedReady()) { onFail?.Invoke(); return; }
        onRewarded = onReward;
        onRewardFailed = onFail;
        PlatformShowRewarded();
    }

    public void ShowInterstitial(Action onDone)
    {
        onInterstitialDone = onDone;
        PlatformShowInterstitial();
    }

    // ===== Common "finish" methods =====

    void GrantReward()
    {
        Action a = onRewarded;
        onRewarded = null;
        onRewardFailed = null;
        a?.Invoke();
    }

    void FailReward()
    {
        Action a = onRewardFailed;
        onRewarded = null;
        onRewardFailed = null;
        a?.Invoke();
    }

    void FinishInterstitial()
    {
        Action a = onInterstitialDone;
        onInterstitialDone = null;
        a?.Invoke();
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    // ===== Android: Unity LevelPlay =====
    LevelPlayRewardedAd rewardedAd;
    LevelPlayInterstitialAd interstitialAd;
    LevelPlayBannerAd bannerAd;
    bool rewardEarned;

    void PlatformInit()
    {
        try
        {
            LevelPlay.OnInitSuccess += OnLevelPlayReady;
            LevelPlay.OnInitFailed += err => Log(ref initStatus, "init FAILED: " + err);
            Log(ref initStatus, "init: starting (key " + appKey + ")");
            LevelPlay.SetAdaptersDebug(true);
            LevelPlay.Init(appKey);
            Invoke(nameof(InitTimeoutCheck), 20f);
        }
        catch (Exception e)
        {
            Log(ref initStatus, "init CRASH: " + e.GetType().Name + " " + e.Message);
        }
    }

    void InitTimeoutCheck()
    {
        if (initStatus.StartsWith("init: starting"))
            Log(ref initStatus, "init: no answer after 20s (internet/SDK problem)");
    }

    void OnLevelPlayReady(LevelPlayConfiguration config)
    {
        Log(ref initStatus, "init: OK");
        CreateBanner(); // the banner shows up as soon as the SDK is ready

        rewardedAd = new LevelPlayRewardedAd(rewardedAdUnitId);
        rewardedAd.OnAdRewarded += (info, reward) => rewardEarned = true;
        rewardedAd.OnAdClosed += info => OnRewardedClosed();
        rewardedAd.OnAdDisplayFailed += (info, err) => OnRewardedClosed();
        rewardedAd.OnAdLoaded += info => Log(ref rewardedStatus, "rewarded: READY");
        rewardedAd.OnAdLoadFailed += err => Log(ref rewardedStatus, "rewarded load failed: " + err);
        Log(ref rewardedStatus, "rewarded: loading...");
        rewardedAd.LoadAd();

        interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);
        interstitialAd.OnAdClosed += info => OnInterstitialClosed();
        interstitialAd.OnAdDisplayFailed += (info, err) => OnInterstitialClosed();
        interstitialAd.OnAdLoaded += info => Log(ref interstitialStatus, "interstitial: READY");
        interstitialAd.OnAdLoadFailed += err => Log(ref interstitialStatus, "interstitial load failed: " + err);
        Log(ref interstitialStatus, "interstitial: loading...");
        interstitialAd.LoadAd();
    }

    void CreateBanner()
    {
        // Default banner (SDK 9.x): 320x50 at the bottom center, displayOnLoad = true,
        // so it shows by itself as soon as it has loaded.
        bannerAd = new LevelPlayBannerAd(bannerAdUnitId);
        bannerAd.OnAdLoaded += info => Log(ref bannerStatus, "banner: loaded");
        bannerAd.OnAdLoadFailed += err => Log(ref bannerStatus, "banner load failed: " + err);
        Log(ref bannerStatus, "banner: loading...");
        bannerAd.LoadAd();
    }

    void OnRewardedClosed()
    {
        if (rewardEarned) GrantReward(); else FailReward();
        rewardEarned = false;
        Log(ref rewardedStatus, "rewarded: loading next...");
        rewardedAd.LoadAd();
    }

    void OnInterstitialClosed()
    {
        interstitialAd.LoadAd();
        FinishInterstitial();
    }

    bool PlatformIsRewardedReady()
    {
        return rewardedAd != null && rewardedAd.IsAdReady();
    }

    void PlatformShowRewarded()
    {
        rewardEarned = false;
        rewardedAd.ShowAd();
    }

    void PlatformShowInterstitial()
    {
        if (interstitialAd != null && interstitialAd.IsAdReady())
            interstitialAd.ShowAd();
        else
            FinishInterstitial();
    }

#elif UNITY_WEBGL && !UNITY_EDITOR
    // ===== WebGL: Monetag through JavaScript (WebAds.jslib) =====
    [DllImport("__Internal")] static extern int WebAd_IsAvailable();
    [DllImport("__Internal")] static extern void WebAd_ShowRewarded();
    [DllImport("__Internal")] static extern void WebAd_ShowInterstitial();

    void PlatformInit() { }
    bool PlatformIsRewardedReady() { return WebAd_IsAvailable() == 1; }
    void PlatformShowRewarded() { WebAd_ShowRewarded(); }
    void PlatformShowInterstitial() { WebAd_ShowInterstitial(); }

#else
    // ===== Editor and PC: stub =====
    void PlatformInit()
    {
        Log(ref initStatus, "editor stub: no real ads");
    }

    bool PlatformIsRewardedReady() { return true; }

    void PlatformShowRewarded()
    {
        Debug.Log("[Ads] Editor stub: reward given at once");
        GrantReward();
    }

    void PlatformShowInterstitial()
    {
        Debug.Log("[Ads] Editor stub: interstitial skipped");
        FinishInterstitial();
    }
#endif

    // JavaScript (WebGL) calls these methods through SendMessage
    public void OnWebRewardedDone() { GrantReward(); }
    public void OnWebRewardedFailed() { FailReward(); }
    public void OnWebInterstitialDone() { FinishInterstitial(); }
}