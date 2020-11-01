using UnityEngine;
using UnityEngine.Advertisements;

[CreateAssetMenu(menuName = "Ads/AdMessagePublisher")]
public class AdMessagePublisher : ScriptableObject
{
    public void ShowInterstitialAd() => Message.Publish(new ShowInterstitialAd());
    public void SetTopLeftBannerAdPosition() => Advertisement.Banner.SetPosition(BannerPosition.TOP_LEFT);
    public void SetBannerAdPosition(BannerPosition position) => Advertisement.Banner.SetPosition(position);
    public void ShowBannerAd(string placementId) => Message.Publish(new ShowBannerAd(placementId));
    public void HideBannerAd() => Message.Publish(new HideBannerAd());
}
