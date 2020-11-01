using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public sealed class UnityAdEngine : OnMessage<ShowInterstitialAd, ShowBannerAd, HideBannerAd>
{
    [SerializeField] private FloatReference adEngineTimeoutSeconds;

    protected override void Execute(ShowInterstitialAd msg)
        => StartCoroutine(ShowWhenReady(
            Advertisement.IsReady, 
            Advertisement.Show));

    protected override void Execute(ShowBannerAd msg) 
        => StartCoroutine(ShowWhenReady(
            () => Advertisement.IsReady(msg.PlacementId), 
            () => Advertisement.Banner.Show(msg.PlacementId)));

    protected override void Execute(HideBannerAd msg)
    {
        Debug.Log("Ads - Hide Banner Ad requested");
        Advertisement.Banner.Hide();
    }

    private IEnumerator ShowWhenReady(Func<bool> isReady, Action action)
    {
        var elapsed = 0f;
        while (!isReady() && elapsed < adEngineTimeoutSeconds) {
            
            elapsed += 0.5f;
            yield return new WaitForSeconds (0.5f);
        }

        if (elapsed >= adEngineTimeoutSeconds)
            Debug.LogWarning("Ads - Unity Ad Engine is not ready.");
        else
            action();
    }
}
