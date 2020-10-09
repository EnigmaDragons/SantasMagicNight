using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class SnowRendererSystem
{
    static SnowRendererSystem m_Instance;
    static public SnowRendererSystem instance {
        get {
            if (m_Instance == null)
                m_Instance = new SnowRendererSystem();
            return m_Instance;
        }
    }

	internal HashSet<SnowOccluderBase> m_ShapeOccluders = new HashSet<SnowOccluderBase>();
    internal HashSet<ForwardPassSnow> m_ForwardSnow = new HashSet<ForwardPassSnow>();

    public void Add (SnowOccluderBase o)
	{
		Remove (o);
        m_ShapeOccluders.Add (o);
	}
	public void Remove (SnowOccluderBase o)
	{
        if (m_ShapeOccluders.Contains(o))
            m_ShapeOccluders.Remove(o);
	}

    public void Add(ForwardPassSnow o)
    {
        Remove(o);
        m_ForwardSnow.Add(o);
    }
    public void Remove(ForwardPassSnow o)
    {
        if (m_ForwardSnow.Contains(o))
            m_ForwardSnow.Remove(o);
    }
}

[ExecuteInEditMode]
public class SnowRenderer : MonoBehaviour
{
    static SnowRenderer m_Instance;
    static public SnowRenderer instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = FindObjectOfType<SnowRenderer>(); // Will return null if the object is disabled
            return m_Instance;
        }
    }

    // Shader passes. Kept up to date to match
    // 'ScreenSpaceSnow.shader' passes
    private enum Pass
    {
        ALBEDO = 0,
        SPECULAR,
        NORMAL,
        LIGHT,
        LIGHT_HDR,
        COPY,
        COUNT
    }

#if UNITY_EDITOR
    [SerializeField] private bool m_AdvancedOptions;
    [SerializeField] private bool m_DebugTextures;
#endif

    // Keep track of if we need to reconstruct the command buffer
    private bool m_IsDirty = true;
    public void MarkDirty()
    {
        m_IsDirty = true;
    }

    private Material m_OcclusionMaterial;
    private Material m_SnowMaterial;
    private Material m_ForwardMaterial;

    /*
     * PBR snow properties:
     * - Albedo (color)
     * - Specular
     * - Normals (tangent space)
     */
    [SerializeField] private bool m_Textured = true;
    [SerializeField] private bool m_TilingFix = false;
    [SerializeField] private bool m_TriplanarProjection = false;

    [SerializeField] private float m_SnowTextureScale = 1.0f;
    [Range(0.0f, 1.0f)]   [SerializeField] private float m_SnowSmoothness = 0.1f;
    [Range(0.0f, 1.0f)]   [SerializeField] private float m_NormalStrength = 1.0f;
    [Range(0.0f, 180.0f)] [SerializeField] private float m_SnowAngle = 90.0f;
    [Range(0.0f, 1.0f)]   [SerializeField] private float m_Gain = 0.5f;

    [SerializeField] private Color m_Tint = Color.white;
    [SerializeField] private Color m_SpecularTint = Color.white;
    [SerializeField] private Color m_SnowClearColor = Color.white;

    [SerializeField] private Texture m_SnowAlbedo;
    [SerializeField] private Texture m_SnowSpecular;
    [SerializeField] private Texture m_SnowNormals;

    #region GETTERS AND SETTERS

    /* BOOLEANS */
    public bool useTextures
    {
        get { return m_Textured; }
        set { m_IsDirty = m_IsDirty || (m_Textured != value); m_Textured = value; }
    }
    public bool useTilingFix
    {
        get { return m_TilingFix; }
        set { m_IsDirty = m_IsDirty || (m_TilingFix != value); m_TilingFix = value; }
    }
    public bool useTriplanarProjectionMapping
    {
        get { return m_TriplanarProjection; }
        set { m_IsDirty = m_IsDirty || (m_TriplanarProjection != value); m_TriplanarProjection = value; }
    }

    /* FLOATS */
    public float snowUVScale
    {
        get { return m_SnowTextureScale; }
        set { m_IsDirty = true; m_SnowTextureScale = value; }
    }
    public float snowSmoothness
    {
        get { return m_SnowSmoothness; }
        set { m_IsDirty = true; m_SnowSmoothness = value; }
    }
    public float normalMapIntensity
    {
        get { return m_NormalStrength; }
        set { m_IsDirty = true; m_NormalStrength = value; }
    }
    public float snowCoverageAngle
    {
        get { return m_SnowAngle; }
        set { m_IsDirty = true; m_SnowAngle = value; }
    }
    public float gain
    {
        get { return m_Gain; }
        set { m_IsDirty = true; m_Gain = value; }
    }

    /* COLORS */
    public Color snowAlbedoColor
    {
        get { return m_Tint; }
        set { m_IsDirty = true; m_Tint = value; }
    }
    public Color snowSpecularColor
    {
        get { return m_SpecularTint; }
        set { m_IsDirty = true; m_SpecularTint = value; }
    }
    public Color snowClearColor
    {
        get { return m_SnowClearColor; }
        set { m_IsDirty = true; m_SnowClearColor = value; }
    }

    /* TEXTURES */
    public Texture snowAlbedo
    {
        get { return m_SnowAlbedo; }
        set { m_IsDirty = true; m_SnowAlbedo = value; }
    }
    public Texture snowSpecular
    {
        get { return m_SnowSpecular; }
        set { m_IsDirty = true; m_SnowSpecular = value; }
    }
    public Texture snowNormalMap
    {
        get { return m_SnowNormals; }
        set { m_IsDirty = true; m_SnowNormals = value; }
    }

    /// <summary>
    /// Get the snow coverage buffer from the specified camera. Returns true if the camera is
    /// being tracked by the snow system. This can be false if the camera is not supported by the system, 
    /// or freshly created and not yet in the system.
    /// </summary>
    /// <param name="cam">Camera with Snow</param>
    /// <param name="renderTex">Coverage Buffer</param>
    /// <returns>True if success, False if failure</returns>
    public bool GetCoverageBuffer(Camera cam, out RenderTexture renderTex)
    {
        if (m_Cameras.ContainsKey(cam))
        {
            renderTex = m_Cameras[cam].m_CoverageBuffer;
            return true;
        }
        renderTex = null;
        return false;
    }

    #endregion

    // We need copies later on so, store the variables 
    private int m_TempAlbedo;
    private int m_TempSpecular;
    private int m_TempNormal;
    private int m_TempEmission;
    private int m_DepthCopy;

    // Before reflections are computed
    private struct snowBufferEntry
	{
        public RenderTexture m_CoverageBuffer;
		public CommandBuffer m_BeforeReflections;
        public CommandBuffer m_BeforeGBuffer;
        public CommandBuffer m_AfterForwardOpaque;
    }

	private Dictionary<Camera,snowBufferEntry> m_Cameras = new Dictionary<Camera,snowBufferEntry>();


	public void OnDisable()
	{
		foreach (var cam in m_Cameras)
		{
			if (cam.Key)
			{
				cam.Key.RemoveCommandBuffer(CameraEvent.BeforeReflections, cam.Value.m_BeforeReflections);
                cam.Key.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, cam.Value.m_BeforeGBuffer);
                cam.Key.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, cam.Value.m_AfterForwardOpaque);
                cam.Value.m_CoverageBuffer.Release();
			}
		}
        m_Cameras.Clear();
        Object.DestroyImmediate(m_OcclusionMaterial);
        Object.DestroyImmediate(m_SnowMaterial);
        Object.DestroyImmediate(m_ForwardMaterial);

        Shader.SetGlobalFloat("_SnowTargetLevel", 0.0f);
        Shader.SetGlobalFloat("_SnowCoverageDeg", 0.0f);

        Camera.onPreRender -= SetVariablesForCamera;
    }

    private void StartEnable()
    {
        if (m_Cameras == null) m_Cameras = new Dictionary<Camera, snowBufferEntry>();
        m_IsDirty = true;

        m_TempAlbedo = Shader.PropertyToID("_TempAlbedo");
        m_TempSpecular = Shader.PropertyToID("_TempSpecular");
        m_TempNormal = Shader.PropertyToID("_TempNormal");
        m_TempEmission = Shader.PropertyToID("_TempEmission");
        m_DepthCopy = Shader.PropertyToID("_DepthCopy");
    }

    private void OnEnable()
    {
        StartEnable();
        Camera.onPreRender += SetVariablesForCamera;
    }

    private void Start()
    {
        StartEnable();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "ScreenSpaceSnow/Snowflake.png", true);
    }

    void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.matrix = transform.localToWorldMatrix;

        // Draw directionality arrows in blue
        DrawArrow(Vector3.forward * -1.5f, Vector3.forward * 3, 0.4f, 30.0f);
        DrawArrow(Vector3.forward * -1.3f + Vector3.up * 0.2f, Vector3.forward * 2.6f, 0.4f, 30.0f);
        DrawArrow(Vector3.forward * -1.3f - Vector3.up * 0.2f, Vector3.forward * 2.6f, 0.4f, 30.0f);
        DrawArrow(Vector3.forward * -1.3f + Vector3.right * 0.2f, Vector3.forward * 2.6f, 0.4f, 30.0f);
        DrawArrow(Vector3.forward * -1.3f - Vector3.right * 0.2f, Vector3.forward * 2.6f, 0.4f, 30.0f);
    }

    public void SetVariablesForCamera(Camera cam)
    {
        if (cam.stereoEnabled)
        {
            // Both stereo eye inverse view matrices
            Matrix4x4 left_world_from_view = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left).inverse;
            Matrix4x4 right_world_from_view = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right).inverse;

            // Both stereo eye inverse projection matrices, plumbed through GetGPUProjectionMatrix to compensate for render texture
            Matrix4x4 left_screen_from_view = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            Matrix4x4 right_screen_from_view = cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
            Matrix4x4 left_view_from_screen = GL.GetGPUProjectionMatrix(left_screen_from_view, true).inverse;
            Matrix4x4 right_view_from_screen = GL.GetGPUProjectionMatrix(right_screen_from_view, true).inverse;

            // Negate [1,1] to reflect Unity's CBuffer state
            left_view_from_screen[1, 1] *= -1;
            right_view_from_screen[1, 1] *= -1;

            // Store matrices
            Shader.SetGlobalMatrix("_SnowLeftWorldFromView", left_world_from_view);
            Shader.SetGlobalMatrix("_SnowRightWorldFromView", right_world_from_view);
            Shader.SetGlobalMatrix("_SnowLeftViewFromScreen", left_view_from_screen);
            Shader.SetGlobalMatrix("_SnowRightViewFromScreen", right_view_from_screen);
        }
        else
        {
            // Main eye inverse view matrix
            Matrix4x4 left_world_from_view = cam.cameraToWorldMatrix;

            // Inverse projection matrices, plumbed through GetGPUProjectionMatrix to compensate for render texture
            Matrix4x4 screen_from_view = cam.projectionMatrix;
            Matrix4x4 left_view_from_screen = GL.GetGPUProjectionMatrix(screen_from_view, true).inverse;

            // Negate [1,1] to reflect Unity's CBuffer state
            left_view_from_screen[1, 1] *= -1;

            // Store matrices
            Shader.SetGlobalMatrix("_SnowLeftWorldFromView", left_world_from_view);
            Shader.SetGlobalMatrix("_SnowLeftViewFromScreen", left_view_from_screen);
        }

        Shader.SetGlobalFloat("_SnowTargetLevel", m_SnowClearColor.r);
        Shader.SetGlobalFloat("_SnowCoverageDeg", Mathf.InverseLerp(0.0f, 180.0f, m_SnowAngle));

        Shader.SetGlobalVector("_SnowFallParameters", new Vector4(
            -transform.forward.x,
            -transform.forward.y,
            -transform.forward.z,
            Mathf.InverseLerp(180.0f, 0.0f, m_SnowAngle) * 2 - 1));


        Shader.SetGlobalVector("_SnowColAndCutoff",
            new Vector4(m_Tint.r, m_Tint.g, m_Tint.b, m_Tint.a));

        Shader.SetGlobalVector("_SnowSpecAndSmoothness",
            new Vector4(m_SpecularTint.r, m_SpecularTint.g, m_SpecularTint.b, m_SnowSmoothness));
        
        Shader.SetGlobalFloat("_SnowGainPower", m_Gain);
    }

    public void RenderSnowForCamera(Camera cam, bool isSceneCam = false)
	{
        if (cam.renderingPath != RenderingPath.DeferredShading)
        {
            // if (cam.name != "Preview Scene Camera" && cam.name != "SceneCamera") // ignore these warnings
            // {
            //     Debug.LogWarningFormat("Screen Space Snow doesn't work with Forward rendering paths. Turning off effect for {0}",
            //         cam.name);
            // }
            return;
        }
    
        var act = gameObject.activeInHierarchy && enabled;
		if (!act)
		{
			OnDisable();
			return;
		}

        // create material used to render lights
        if (!m_OcclusionMaterial)
        {
            m_OcclusionMaterial = new Material(Shader.Find("Hidden/ScreenSpaceSnowOccluders"));
            m_OcclusionMaterial.name = "Occlusion Material";
            m_OcclusionMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
        if (!m_SnowMaterial)
        {
            m_SnowMaterial = new Material(Shader.Find("Hidden/ScreenSpaceSnow"));
            m_SnowMaterial.name = "Snow Material";
            m_SnowMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
        if (!m_ForwardMaterial)
        {
            m_ForwardMaterial = new Material(Shader.Find("Hidden/ScreenSpaceSnowForward"));
            m_ForwardMaterial.name = "Forward Snow Material";
            m_ForwardMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
        
        snowBufferEntry buf = new snowBufferEntry();
		if (m_Cameras.ContainsKey(cam))
		{
			// use existing command buffers: clear them
			buf = m_Cameras[cam];

            if (buf.m_CoverageBuffer.width != cam.pixelWidth || buf.m_CoverageBuffer.height != cam.pixelHeight)
            {
                // Used only for soft fade stuff. 256 values should be enough, can increase it here though
                buf.m_CoverageBuffer = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.R8);
            }

            buf.m_BeforeReflections.Clear ();
            buf.m_BeforeGBuffer.Clear();
            buf.m_AfterForwardOpaque.Clear();

            m_Cameras[cam] = buf;
        }
		else
		{
			// create new command buffers
			buf.m_BeforeReflections = new CommandBuffer();
			buf.m_BeforeReflections.name = "Screen Space Snow - Write Snow";
    
            buf.m_BeforeGBuffer = new CommandBuffer();
            buf.m_BeforeGBuffer.name = "Screen Space Snow - Clear Coverage";

            buf.m_AfterForwardOpaque = new CommandBuffer();
            buf.m_AfterForwardOpaque.name = "Screen Space Snow - Forward Renderers";

            // Used only for soft fade stuff. 256 values should be enough, can increase it here though
            buf.m_CoverageBuffer = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.R8);
    
            m_Cameras[cam] = buf;
    
			cam.AddCommandBuffer(CameraEvent.BeforeReflections, buf.m_BeforeReflections);
            cam.AddCommandBuffer(CameraEvent.BeforeGBuffer, buf.m_BeforeGBuffer);
            cam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, buf.m_AfterForwardOpaque);
        }
    
		var system = SnowRendererSystem.instance;

        // Use shader here, so any shader can access the coverage buffer
        Shader.SetGlobalTexture("_Coverage", buf.m_CoverageBuffer);

        // Clear this buffer as soon as possible
        buf.m_BeforeGBuffer.SetRenderTarget(buf.m_CoverageBuffer);
        buf.m_BeforeGBuffer.ClearRenderTarget(false, true, m_SnowClearColor);

        buf.m_BeforeReflections.GetTemporaryRT(m_DepthCopy, -1, -1, 0, FilterMode.Point, RenderTextureFormat.RFloat);
        buf.m_BeforeReflections.SetRenderTarget(new RenderTargetIdentifier(m_DepthCopy));
        buf.m_BeforeReflections.ClearRenderTarget(false, true, Color.black);
        buf.m_BeforeReflections.DrawMesh(ProceduralPrimitives.blitQuad, Matrix4x4.identity, m_OcclusionMaterial, 0, 3);

        if (cam.stereoEnabled)
            buf.m_BeforeReflections.SetRenderTarget(buf.m_CoverageBuffer);
        else if (!isSceneCam)
            buf.m_BeforeReflections.SetRenderTarget(buf.m_CoverageBuffer, new RenderTargetIdentifier(BuiltinRenderTextureType.ResolvedDepth));
        else
            buf.m_BeforeReflections.SetRenderTarget(buf.m_CoverageBuffer);

        foreach (var o in system.m_ShapeOccluders)   // Adding custom shapes
        {
            buf.m_BeforeReflections.SetGlobalFloat("_SnowAmount", o.m_SnowAmount);
            buf.m_BeforeReflections.SetGlobalVector("_FeatherScale", new Vector4(
                o.m_FeatherScale.x,
                o.m_FeatherScale.y,
                o.m_FeatherScale.z, 
                o.m_Radius));
            
            buf.m_BeforeReflections.DrawRenderer(o.m_Renderer, m_OcclusionMaterial, 0, (int)o.m_Shape);
        }
        buf.m_BeforeReflections.ReleaseTemporaryRT(m_DepthCopy);

        // Set the samplers
        buf.m_BeforeReflections.SetGlobalTexture("_AlbedoTex",
#if UNITY_EDITOR
            m_DebugTextures && isSceneCam ? Resources.Load("DebugColors") as Texture :
# endif
            m_SnowAlbedo);
        buf.m_BeforeReflections.SetGlobalTexture("_SpecularTex", m_SnowSpecular);
        buf.m_BeforeReflections.SetGlobalTexture("_NormalsTex", m_SnowNormals);
        buf.m_BeforeReflections.SetGlobalTexture("_Coverage", buf.m_CoverageBuffer);

        // Set shader keywords
        {
            if (m_TilingFix)
                buf.m_BeforeReflections.EnableShaderKeyword("TILING_FIX");
            else
                buf.m_BeforeReflections.DisableShaderKeyword("TILING_FIX");

#if UNITY_EDITOR
            if (m_Textured ||(m_DebugTextures && isSceneCam))
                buf.m_BeforeReflections.EnableShaderKeyword("TEXTURED");
            else
                buf.m_BeforeReflections.DisableShaderKeyword("TEXTURED");
#else
            if (m_Textured)
                buf.m_BeforeReflections.EnableShaderKeyword("TEXTURED");
            else
                buf.m_BeforeReflections.DisableShaderKeyword("TEXTURED");
#endif

            if (m_TriplanarProjection)
                buf.m_BeforeReflections.EnableShaderKeyword("TRIPLANAR_PROJ");
            else
                buf.m_BeforeReflections.DisableShaderKeyword("TRIPLANAR_PROJ");
        }

        // Set the parameters
        buf.m_BeforeReflections.SetGlobalFloat("_SnowTextureScale", 1.0f / m_SnowTextureScale);
        buf.m_BeforeReflections.SetGlobalFloat("_NormalStrength",
#if UNITY_EDITOR
            m_DebugTextures && isSceneCam ? 0 : 
#endif
            m_NormalStrength);

        // Need to make a temporary copy of the G buffer, because we can't render to the textures we're sampling
        // Render the current G buffer into our temporary working buffers. Everything gets modified so, we need a copy of everything
        {
            /* -- NORMALS -- */
            buf.m_BeforeReflections.GetTemporaryRT(m_TempNormal, cam.pixelWidth, cam.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010);
            buf.m_BeforeReflections.Blit(new RenderTargetIdentifier(BuiltinRenderTextureType.GBuffer2), m_TempNormal, m_SnowMaterial, (int)Pass.COPY);
            buf.m_BeforeReflections.Blit(m_TempNormal, new RenderTargetIdentifier(BuiltinRenderTextureType.GBuffer2), m_SnowMaterial, (int)Pass.NORMAL);
            /* -- ALBEDO -- */
            buf.m_BeforeReflections.GetTemporaryRT(m_TempAlbedo, cam.pixelWidth, cam.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
            buf.m_BeforeReflections.Blit(new RenderTargetIdentifier(BuiltinRenderTextureType.GBuffer0), m_TempAlbedo, m_SnowMaterial, (int)Pass.COPY);
            buf.m_BeforeReflections.Blit(m_TempAlbedo, new RenderTargetIdentifier(BuiltinRenderTextureType.GBuffer0), m_SnowMaterial, (int)Pass.ALBEDO);
            buf.m_BeforeReflections.ReleaseTemporaryRT(m_TempAlbedo);
            /* -- SPECULAR -- */
            buf.m_BeforeReflections.GetTemporaryRT(m_TempSpecular, cam.pixelWidth, cam.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
            buf.m_BeforeReflections.Blit(new RenderTargetIdentifier(BuiltinRenderTextureType.GBuffer1), m_TempSpecular, m_SnowMaterial, (int)Pass.COPY);
            buf.m_BeforeReflections.Blit(m_TempSpecular, new RenderTargetIdentifier(BuiltinRenderTextureType.GBuffer1), m_SnowMaterial, (int)Pass.SPECULAR);
            buf.m_BeforeReflections.ReleaseTemporaryRT(m_TempSpecular);
            
            /* -- LIGHTING -- */
            if (!cam.allowHDR)
            {
                buf.m_BeforeReflections.GetTemporaryRT(m_TempEmission, cam.pixelWidth, cam.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010);
                buf.m_BeforeReflections.Blit(new RenderTargetIdentifier(BuiltinRenderTextureType.GBuffer3), m_TempEmission, m_SnowMaterial, (int)Pass.COPY);
                buf.m_BeforeReflections.Blit(m_TempEmission, new RenderTargetIdentifier(BuiltinRenderTextureType.GBuffer3), m_SnowMaterial, (int)Pass.LIGHT);
                buf.m_BeforeReflections.ReleaseTemporaryRT(m_TempEmission);
            }
            else
            {
                Debug.LogWarningFormat("HDR Snow not fully supported. Please switch to LDR, on Camera {0}", cam.name);
                buf.m_BeforeReflections.SetRenderTarget(new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));
                buf.m_BeforeReflections.ClearRenderTarget(false, true, Color.black);
            }

            /* -- NORMALS -- */
            // Release normals last, we use them in other calculations
            buf.m_BeforeReflections.ReleaseTemporaryRT(m_TempNormal);
        }

        foreach (var o in system.m_ForwardSnow)   // Adding custom shapes
        {
            buf.m_AfterForwardOpaque.DrawRenderer(o.m_Renderer, m_ForwardMaterial, 0);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        m_SnowMaterial = null;
        m_ForwardMaterial = null;
        m_OcclusionMaterial = null;
        m_IsDirty = true;

        foreach (Camera cam in UnityEditor.SceneView.GetAllSceneCameras())
        {   // Also sets global variables
            SetVariablesForCamera(UnityEditor.SceneView.GetAllSceneCameras()[0]);
        }
    }
#endif

    public void Update()
    {
#if UNITY_EDITOR
        foreach (Camera cam in UnityEditor.SceneView.GetAllSceneCameras())
        {
            if (transform.hasChanged || m_IsDirty || !m_Cameras.ContainsKey(cam) ||
                m_Cameras[cam].m_CoverageBuffer.width != cam.pixelWidth || m_Cameras[cam].m_CoverageBuffer.height != cam.pixelHeight)
            {
                RenderSnowForCamera(cam, true);
            }
        }
#endif

        foreach (Camera cam in Camera.allCameras)
        {
            if (transform.hasChanged || m_IsDirty || !m_Cameras.ContainsKey(cam) ||
                m_Cameras[cam].m_CoverageBuffer.width != cam.pixelWidth || m_Cameras[cam].m_CoverageBuffer.height != cam.pixelHeight)
            {
                RenderSnowForCamera(cam);
            }
        }
    
        m_IsDirty = false;
    }
}
