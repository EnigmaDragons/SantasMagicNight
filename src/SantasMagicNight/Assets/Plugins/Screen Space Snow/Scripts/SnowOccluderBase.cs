using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public abstract class SnowOccluderBase : MonoBehaviour
{
    // Types of occluder shapes,
    // also corrolates with the passes in the
    // occlusion shader
    public enum Shape
    {
        MESH = 0,
        BOX = 1,
        SPHERE = 2,
    }

    // We need to mark the renderer as dirty if any properties change that affect the snow
    [Range(0.0f, 1.0f)]
    [SerializeField] private float _SnowAmount;
    public float m_SnowAmount
    {
        get { return _SnowAmount; }
        set { if (value != _SnowAmount) SnowRenderer.instance.MarkDirty(); _SnowAmount = value; }
    }
    [SerializeField] private Renderer _Renderer;
    public Renderer m_Renderer
    {
        get { return _Renderer; }
        set { if (value != _Renderer) SnowRenderer.instance.MarkDirty(); _Renderer = value; }
    }

    [SerializeField] private Shape _Shape = Shape.MESH;
    public Shape m_Shape
    {
        get { return _Shape; }
        set
        {
            if ((_Shape == Shape.BOX || _Shape == Shape.SPHERE) && value == Shape.MESH)
                Debug.LogWarning("Setting Mesh occluder to Box or Sphere overwrites the shared mesh with a primitive");

            if (value != _Shape) SnowRenderer.instance.MarkDirty();
            _Shape = value;

            if (_Shape == Shape.BOX || _Shape == Shape.SPHERE)
            {
                GetComponent<MeshFilter>().sharedMesh =
                    m_Shape == Shape.BOX ? ProceduralPrimitives.occluderCube :
                    ProceduralPrimitives.occluderSphere;
            }
        }
    }

    [SerializeField] private float _Feather = 0.25f; // Both shapes
    public float m_Feather
    {
        get { return _Feather; }
        set { if (value != _Feather) SnowRenderer.instance.MarkDirty(); _Feather = value; }
    }
    [SerializeField] private float _Radius = 1.0f; // Sphere only
    public float m_Radius
    {
        get { return _Radius; }
        set { if (value != _Radius) SnowRenderer.instance.MarkDirty(); _Radius = value; }
    }

    [SerializeField] private Vector3 _FeatherScale;
    public Vector3 m_FeatherScale
    {
        get { return _FeatherScale; }
        set { if (value != _FeatherScale) SnowRenderer.instance.MarkDirty(); _FeatherScale = value; }
    }

    public abstract void Enable();
    public abstract void OnStart();

    public void Start()
    {
        OnStart();
        SnowRendererSystem.instance.Add(this);
        if (SnowRenderer.instance)  // This can be null if it doesn't exist or is disabled
            SnowRenderer.instance.MarkDirty();
    }

    public void OnEnable()
    {
        Enable();
        SnowRendererSystem.instance.Add(this);
        if (SnowRenderer.instance)
            SnowRenderer.instance.MarkDirty();
    }

    public void OnDisable()
    {
        SnowRendererSystem.instance.Remove(this);

        if (SnowRenderer.instance)
            SnowRenderer.instance.MarkDirty();
    }
}
