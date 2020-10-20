
using UnityEngine;

[CreateAssetMenu(menuName = "Santa/EventPublisher")]
public class SantaEventPublisher : ScriptableObject
{
    public void ShowOptionsMenu() => Message.Publish(new ShowOptionsMenu());
    public void ShowLevelSelect() => Message.Publish(new ShowLevelSelect());
    public void HideCurrentSubMenu() => Message.Publish(new HideCurrentSubMenu());
}
