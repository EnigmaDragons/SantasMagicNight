using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SnowRenderer))]
public class SnowRendererEditor : Editor
{
    /* BOOLEANS */
    SerializedProperty isTextured;
    SerializedProperty snowClearColor;
    SerializedProperty tilingFix;
    SerializedProperty triplanarProjection;

    /* FLOATS */
    SerializedProperty snowTextureScale;
    SerializedProperty snowSmoothness;
    SerializedProperty normalsIntensity;
    SerializedProperty coverageAmount;

    /* COLORS AND TEXTURES */
    SerializedProperty tint;
    SerializedProperty specularTint;
    SerializedProperty snowAlbedo;
    SerializedProperty snowSpecular;
    SerializedProperty snowNormals;

    /* EDITOR STUFF */
    SerializedProperty advancedOptions;
    SerializedProperty debugTextures;

    SnowRenderer snowRenderer;

    [MenuItem("GameObject/Snow/Snow", false, 11)]
    public static void CreateSnowRenderer()
    {
        GameObject snow = (Resources.Load("Prefabs/SnowRenderer") as GameObject);
        GameObject created = Instantiate(snow);
        var mf = created.GetComponent<MeshFilter>();
        mf.sharedMesh = ProceduralPrimitives.boundsCube;
        created.name = "World Snow";
    }

    void OnEnable()
    {
        snowRenderer = target as SnowRenderer;

        isTextured = serializedObject.FindProperty("m_Textured");
        snowClearColor = serializedObject.FindProperty("m_SnowClearColor");
        tilingFix = serializedObject.FindProperty("m_TilingFix");
        triplanarProjection = serializedObject.FindProperty("m_TriplanarProjection");

        snowTextureScale = serializedObject.FindProperty("m_SnowTextureScale");
        snowSmoothness = serializedObject.FindProperty("m_SnowSmoothness");
        normalsIntensity = serializedObject.FindProperty("m_NormalStrength");
        coverageAmount = serializedObject.FindProperty("m_SnowAngle");

        tint = serializedObject.FindProperty("m_Tint");
        specularTint = serializedObject.FindProperty("m_SpecularTint");
        snowAlbedo = serializedObject.FindProperty("m_SnowAlbedo");
        snowSpecular = serializedObject.FindProperty("m_SnowSpecular");
        snowNormals = serializedObject.FindProperty("m_SnowNormals");

        advancedOptions = serializedObject.FindProperty("m_AdvancedOptions");
        debugTextures = serializedObject.FindProperty("m_DebugTextures");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        using (var mainBody = new EditorGUILayout.VerticalScope("button"))
        {
            GUILayout.Label("Snow Settings", EditorStyles.boldLabel);

            GUIStyle tallToolbar = new GUIStyle("toolbar") { fixedHeight = 20f };

            using (var horizontalLabel = new EditorGUILayout.HorizontalScope(tallToolbar))
            {
                if (GUI.Button(horizontalLabel.rect, GUIContent.none, GUIStyle.none))
                    isTextured.boolValue = !isTextured.boolValue;

                EditorGUILayout.Toggle(isTextured.boolValue, GUILayout.Width(20f));
                GUILayout.Label("Textured Snow");
                GUILayout.FlexibleSpace();
            }

            EditorGUI.BeginDisabledGroup(!isTextured.boolValue);
            {
                float imageWidth = ((Screen.width - 60f) / 3f) - 5f;
                using (var imageBox = new EditorGUILayout.HorizontalScope())
                {
                    float x = imageBox.rect.x + 5f;
                    float y = imageBox.rect.y + 20f;

                    var guiCentered = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleCenter };

                    GUILayout.Label("Albedo Map", guiCentered, GUILayout.Width(imageWidth + 13f));
                    GUILayout.Label("Specular Map", guiCentered, GUILayout.Width(imageWidth + 13f));
                    GUILayout.Label("Normal Map", guiCentered, GUILayout.Width(imageWidth + 13f));

                    EditorGUI.DrawPreviewTexture(new Rect(x, y, imageWidth, imageWidth), snowRenderer.snowAlbedo); x += imageWidth + 18f;
                    EditorGUI.DrawPreviewTexture(new Rect(x, y, imageWidth, imageWidth), snowRenderer.snowSpecular); x += imageWidth + 18f;
                    EditorGUI.DrawPreviewTexture(new Rect(x, y, imageWidth, imageWidth), snowRenderer.snowNormalMap); x += imageWidth + 18f;

                }
                GUILayout.Space(Screen.width / 3f);

                EditorGUILayout.PropertyField(snowTextureScale, new GUIContent(
                        "Snow Texture Scale",
                        "Scale of the snow texture projected on the world"));
                EditorGUILayout.PropertyField(snowAlbedo, new GUIContent(
                        "Albedo",
                        "Albedo (RGB) and Transparency (A)"));

                EditorGUILayout.PropertyField(snowSpecular, new GUIContent(
                        "Specular Map",
                        "Specular (RGB) and Smoothness (A)"));

                EditorGUILayout.PropertyField(snowNormals, new GUIContent(
                        "Normal Map", "Normal Map"));
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(4f);

            using (var horizontalLabel = new EditorGUILayout.HorizontalScope(tallToolbar))
            {
                GUILayout.Label("Color Settings");
            }

            EditorGUILayout.PropertyField(tint, new GUIContent(
                    "Albedo Color",
                    "(RGB) Albedo color"));
            EditorGUILayout.PropertyField(specularTint, new GUIContent(
                    "Specular Color",
                    "(RGB) Specular color"));
            EditorGUILayout.PropertyField(snowSmoothness, new GUIContent(
                    "Smoothness",
                    "Smoothness"));

            GUILayout.Space(4f);

            using (var horizontalLabel = new EditorGUILayout.HorizontalScope(tallToolbar))
            {
                if (GUI.Button(horizontalLabel.rect, GUIContent.none, GUIStyle.none))
                    advancedOptions.boolValue = !advancedOptions.boolValue;

                GUILayout.Label(advancedOptions.boolValue ? downIcon : rightIcon);
                GUILayout.Label("Advanced Settings");

                GUILayout.FlexibleSpace();
                GUILayout.Label(gearIcon);
            }

            if (advancedOptions.boolValue)
            {
                float color = EditorGUILayout.Slider(new GUIContent("Clear Amount",
                        "How much snow is already in the coverage buffer"),
                        snowClearColor.colorValue.r, 0.0f, 1.0f);
                snowClearColor.colorValue = Color.white * color;

                EditorGUILayout.PropertyField(normalsIntensity, new GUIContent(
                    "Normals Intensity",
                    "Scales the intensity of the normal map, making more (1.0f) or less (0.0f) bumpy"));
                EditorGUILayout.PropertyField(coverageAmount, new GUIContent(
                    "Coverage (in °)",
                    "How many degrees of coverage will the snow cover. 180° is maximum," +
                    " 90° is default, and 0° is no coverage"));

                EditorGUI.BeginDisabledGroup(!isTextured.boolValue);
                {
                    EditorGUILayout.PropertyField(tilingFix, new GUIContent(
                    "Tiling Fix",
                    "Prevents repetitive textures from forming over large areas"));
                    EditorGUILayout.PropertyField(triplanarProjection, new GUIContent(
                        "Triplanar Projection",
                        "Fixes streaks caused by a single projection map being used on " +
                        "surfaces perpendicular to the snow direction"));
                }
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.PropertyField(debugTextures, new GUIContent(
                    "Debug View",
                    "Use a grid texture to see various effects clearly"));
            }

            GUILayout.Space(4f);

            using (var horizontalLabel = new EditorGUILayout.HorizontalScope(tallToolbar))
            {
                if (GUI.Button(horizontalLabel.rect, GUIContent.none, GUIStyle.none)) Application.OpenURL("mailto:davidarppebusiness@gmail.com?subject=Screen%20Space%20Snow%20-%20Questions");
                GUILayout.Label("Questions? Send us an eMail!");
                GUILayout.FlexibleSpace();
                GUILayout.Label(helpIcon);
            }

            GUILayout.Space(4f);

            using (var horizontalLabel = new EditorGUILayout.HorizontalScope(tallToolbar))
            {
                if (GUI.Button(horizontalLabel.rect, GUIContent.none, GUIStyle.none)) Application.OpenURL("https://assetstore.unity.com/packages/vfx/shaders/screen-space-snow-106001");

                GUILayout.Label("Leave us a review!");
                GUILayout.FlexibleSpace();

                for (int i = 0; i < 5; i++) GUILayout.Label(starIcon);
            }
        }
       
        serializedObject.ApplyModifiedProperties();
    }

    private static Texture _barsIcon = null;
    protected static Texture barsIcon
    {
        get
        {
            if (_barsIcon == null) _barsIcon = Resources.Load<Texture>("editorBars");
            return _barsIcon;
        }
    }

    private static Texture _downIcon = null;
    protected static Texture downIcon
    {
        get
        {
            if (_downIcon == null) _downIcon = Resources.Load<Texture>("editorDown");
            return _downIcon;
        }
    }

    private static Texture _helpIcon = null;
    protected static Texture helpIcon
    {
        get
        {
            if (_helpIcon == null) _helpIcon = Resources.Load<Texture>("editorHelp");
            return _helpIcon;
        }
    }

    private static Texture _rightIcon = null;
    protected static Texture rightIcon
    {
        get
        {
            if (_rightIcon == null) _rightIcon = Resources.Load<Texture>("editorRight");
            return _rightIcon;
        }
    }

    private static Texture _gearIcon = null;
    protected static Texture gearIcon
    {
        get
        {
            if (_gearIcon == null) _gearIcon = Resources.Load<Texture>("editorGear");
            return _gearIcon;
        }
    }

    private static Texture _starIcon = null;
    protected static Texture starIcon
    {
        get
        {
            if (_starIcon == null) _starIcon = Resources.Load<Texture>("editorStar");
            return _starIcon;
        }
    }
}