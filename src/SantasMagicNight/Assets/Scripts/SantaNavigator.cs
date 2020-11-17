using UnityEngine;

[CreateAssetMenu(menuName = "Santa/Navigator")]
public class SantaNavigator : ScriptableObject
{
    public void NavigateToGameScene() => Message.Publish(new NavigateToSceneRequested("GameScene"));
    public void NavigateToCredits() => Message.Publish(new NavigateToSceneRequested("CreditsScene"));
    public void NavigateToMainMenu() =>  Message.Publish(new NavigateToSceneRequested("MainMenu"));

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
