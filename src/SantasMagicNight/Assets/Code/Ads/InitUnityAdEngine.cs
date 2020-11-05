using UnityEngine;
using UnityEngine.Advertisements;

public sealed class InitUnityAdEngine : MonoBehaviour
{
    [SerializeField] private SecureStringVariable appleGameId;
    [SerializeField] private SecureStringVariable googleGameId;
    
    private bool isTestMode = true;
    
    private void Start()
    {
        var gameId = "";
        #if UNITY_ANDROID
            gameId = googleGameId;
        #elif UNITY_IOS
            gameId = appleGameId;
        #endif
        
        if (!string.IsNullOrWhiteSpace(gameId))
            Advertisement.Initialize(gameId.Trim(), isTestMode);
    }
}
