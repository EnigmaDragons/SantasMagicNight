
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Santa/Navigator")]
public class SantaNavigator : ScriptableObject
{
    public void NavigateToGameScene() => SceneManager.LoadScene("GameScene");
    public void NavigateToCredits() => SceneManager.LoadScene("CreditsScene");
    public void NavigateToMainMenu() => SceneManager.LoadScene("MainMenu");
}
