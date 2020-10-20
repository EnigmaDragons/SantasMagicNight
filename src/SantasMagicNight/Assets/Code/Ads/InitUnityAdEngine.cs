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
        #if UNITY_EDITOR
        #elif UNITY_ANDROID
            gameId = googleGameId;
        #elif UNITY_IOS
            gameId = appleGameId;
        #endif
        
        Advertisement.Initialize(gameId, isTestMode);
    }
}
