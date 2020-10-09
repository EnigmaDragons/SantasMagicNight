using UnityEngine;

[ExecuteInEditMode]
public class ForwardPassSnow : MonoBehaviour
{
    [HideInInspector] public Renderer m_Renderer;

    public void OnEnable()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        if (m_Renderer == null)
        {
            m_Renderer = GetComponent<SkinnedMeshRenderer>();
            if (m_Renderer == null)
            {
                Debug.LogWarningFormat("Renderer not added to the forward snow system, there is no " +
                    "MeshRenderer or SkinnedMeshRenderer on {0}.",
                    gameObject.name);
            }
        }

        SnowRendererSystem.instance.Add(this);
        SnowRenderer.instance.MarkDirty();
    }

    public void OnDisable()
    {
        SnowRendererSystem.instance.Remove(this);
        SnowRenderer.instance.MarkDirty();
    }

    public void Start()
    {
        OnEnable();
    }
}
