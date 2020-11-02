using UnityEditor;

public sealed class RefreshLevelAssets : UnityEditor.Editor
{
    [MenuItem("Tools/Refresh Level Assets")]
    static void Execute()
    {
        var levels = UnityResourceUtils.FindAssetsByType<GameLevel>();
        levels.ForEach(EditorUtility.SetDirty);
    }
}
