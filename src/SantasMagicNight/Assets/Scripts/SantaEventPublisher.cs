
using UnityEngine;

[CreateAssetMenu(menuName = "Santa/EventPublisher")]
public class SantaEventPublisher : ScriptableObject
{
    public void ShowOptionsMenu() => Message.Publish(new ShowOptionsMenu());
    public void HideOptionsMenu() => Message.Publish(new HideOptionsMenu());
}
