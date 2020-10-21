
using UnityEngine;

[CreateAssetMenu(menuName = "Santa/EventPublisher")]
public class SantaEventPublisher : ScriptableObject
{
    public void ShowOptionsMenu() => Message.Publish(new ShowOptionsMenu());
    public void ShowLevelSelect() => Message.Publish(new ShowLevelSelect());
    public void HideCurrentSubMenu() => Message.Publish(new HideCurrentSubMenu());
    public void GoToNextLevel() => Message.Publish(new GoToNextLevel());
    public void RetryLevel() => Message.Publish(new RetryLevel());
}
