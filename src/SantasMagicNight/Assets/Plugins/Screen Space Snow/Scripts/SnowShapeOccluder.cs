using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Renderer))]
public class SnowShapeOccluder : SnowOccluderBase
{
    public override void Enable()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        if (m_Renderer == null)
        {
            Debug.LogWarningFormat("Renderer not added to the snow occlusion system, there is no " +
                "MeshRenderer on {0}. Try using one of the prefabs if you're unsure.",
                gameObject.name);
        }

        if (GetComponent<MeshFilter>() == null) gameObject.AddComponent<MeshFilter>();
        m_Renderer.enabled = false;
        GetComponent<MeshFilter>().sharedMesh =
            m_Shape == Shape.BOX ? ProceduralPrimitives.occluderCube :
            ProceduralPrimitives.occluderSphere;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        m_Renderer.enabled = false;

        m_Feather = Mathf.Max(m_Feather, 0.0f);
    }
#endif

    private float m_OldFeather; // used for keeping track of changes
    private float m_OldRadius; // used for keeping track of changes

    public override void OnStart()
    {
        Enable();
    }

    public void Update()
    {
        if (!transform.hasChanged && m_Feather == m_OldFeather && m_Radius == m_OldRadius) return;
        m_OldRadius = m_Radius; m_OldFeather = m_Feather;
        
        if (m_Shape == Shape.BOX)
        {
            Vector3 lScale = transform.lossyScale;
            m_FeatherScale = new Vector3(
                (lScale.x - m_Feather * 2.0f) / lScale.x,
                (lScale.y - m_Feather * 2.0f) / lScale.y, 
                (lScale.z - m_Feather * 2.0f) / lScale.z);

            float x = Mathf.Max(m_FeatherScale.x, 0.0f);
            float y = Mathf.Max(m_FeatherScale.y, 0.0f);
            float z = Mathf.Max(m_FeatherScale.z, 0.0f);

            m_FeatherScale = new Vector3(x, y, z);
        }
        else if (m_Shape == Shape.SPHERE)
        {
            transform.localScale = Vector3.one * m_Radius;
            m_FeatherScale = Vector3.one * ((m_Radius - m_Feather) / m_Radius);
        }
    }

    public void OnDrawGizmos()
	{
        if (m_Shape == Shape.BOX)
        {
            Gizmos.matrix = gameObject.transform.localToWorldMatrix * Matrix4x4.Scale(m_FeatherScale);
            Gizmos.color = new Color(1, 0, 0);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

            Gizmos.matrix = gameObject.transform.localToWorldMatrix;
            Gizmos.color = new Color(0, 1, 0);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
        else if (m_Shape == Shape.SPHERE)
        {
            Gizmos.matrix = Matrix4x4.identity * Matrix4x4.Translate(transform.position) * Matrix4x4.Scale(m_FeatherScale * m_Radius);
            Gizmos.color = new Color(1, 0, 0);
            Gizmos.DrawWireSphere(Vector3.zero, 1.0f);

            Gizmos.matrix = Matrix4x4.identity * Matrix4x4.Translate(transform.position) * Matrix4x4.Scale(Vector3.one * m_Radius);
            Gizmos.color = new Color(0, 1, 0);
            Gizmos.DrawWireSphere(Vector3.zero, 1.0f);
        }
    }
}
