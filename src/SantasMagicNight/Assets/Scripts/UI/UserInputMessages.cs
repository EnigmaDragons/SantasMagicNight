using UnityEngine;

[CreateAssetMenu(menuName = "Infrastructure/UserInputMessages")]
public sealed class UserInputMessages : ScriptableObject
{
    public void RequestLevelReset() => Message.Publish(new LevelResetRequested());
    public void RequestUndo() => Message.Publish(new UndoRequested());
}
