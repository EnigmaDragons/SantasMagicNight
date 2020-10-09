using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SnowShapeOccluder))]
public class SnowShapeOccluderEditor : Editor
{
    /* FLOATS */
    SerializedProperty snowAmount;
    SerializedProperty feather;
    SerializedProperty radius;

    /* OPTION */
    SerializedProperty shape;

    [MenuItem("GameObject/Snow/Snow Box Occluder", false, 11)]
    public static void CreateBox()
    {
        GameObject snow = (Resources.Load("Prefabs/SnowBoxOccluder") as GameObject);
        GameObject created = Instantiate(snow);
        if (SceneView.GetAllSceneCameras()[0] != null)
            created.transform.position = 
                SceneView.GetAllSceneCameras()[0].transform.position + 
                SceneView.GetAllSceneCameras()[0].transform.forward * 8.0f;
        created.name = "Snow Box Occluder";
    }

    [MenuItem("GameObject/Snow/Snow Sphere Occluder", false, 11)]
    public static void CreateSphere()
    {
        GameObject snow = (Resources.Load("Prefabs/SnowSphereOccluder") as GameObject);
        GameObject created = Instantiate(snow);
        if (SceneView.GetAllSceneCameras()[0] != null)
            created.transform.position = 
                SceneView.GetAllSceneCameras()[0].transform.position + 
                SceneView.GetAllSceneCameras()[0].transform.forward * 8.0f;
        created.name = "Snow Sphere Occluder";
    }

    void OnEnable()
    {
        snowAmount = serializedObject.FindProperty("_SnowAmount");
        feather = serializedObject.FindProperty("_Feather");
        radius = serializedObject.FindProperty("_Radius");

        shape = serializedObject.FindProperty("_Shape");
    }

    string[] options = new string[]
    {
        "Box Occluder", "Sphere Occluder"
    };

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        {
            using (var verticalLayout = new EditorGUILayout.VerticalScope("button"))
            {
                GUILayout.Label("Occluder Shape Settings", EditorStyles.boldLabel);

                shape.intValue = EditorGUILayout.Popup("Shape", shape.intValue - 1, options) + 1;

                EditorGUILayout.PropertyField(snowAmount, new GUIContent(
                        "Snow Amount",
                        "0 is remove snow, and 1 is add snow (useful if [Default Snow On] "+
                        "is turned off for the SnowRenderer)"));
                EditorGUILayout.PropertyField(feather, new GUIContent(
                        "Feather Distance",
                        "In world space, the distance for the Hermite falloff " +
                        "used by the occluder"));

                if (shape.intValue == (int)SnowOccluderBase.Shape.SPHERE)
                {
                    EditorGUILayout.PropertyField(radius, new GUIContent(
                            "Sphere Radius",
                            "In world space, the radius of the sphere occluder"));
                }
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}