using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SnowMeshOccluder)), CanEditMultipleObjects]
public class SnowMeshOccluderEditor : Editor
{
    /* FLOATS */
    SerializedProperty snowAmount;

    [MenuItem("Component/Snow/Snow Mesh Occluder")]
    public static void CreateSphere()
    {
        Selection.activeGameObject.AddComponent<SnowMeshOccluder>();
    }

    void OnEnable()
    {
        snowAmount = serializedObject.FindProperty("_SnowAmount");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        {
            EditorGUILayout.PropertyField(snowAmount, new GUIContent(
                    "Snow Amount",
                    "0 is remove snow, and 1 is add snow (useful if [Default Snow On] "+
                    "is turned off for the SnowRenderer)"));
        }
        serializedObject.ApplyModifiedProperties();
    }
}