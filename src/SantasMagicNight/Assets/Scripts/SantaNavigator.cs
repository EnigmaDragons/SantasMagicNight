using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Santa/Navigator")]
public class SantaNavigator : ScriptableObject
{
    public void NavigateToGameScene() => SceneManager.LoadScene("GameScene");
    public void NavigateToCredits() => SceneManager.LoadScene("CreditsScene");
    public void NavigateToMainMenu() => SceneManager.LoadScene("MainMenu");

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
