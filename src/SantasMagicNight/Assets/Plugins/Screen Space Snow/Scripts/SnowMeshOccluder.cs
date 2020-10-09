using UnityEngine;

public class SnowMeshOccluder : SnowOccluderBase
{
    public override void Enable()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        if (m_Renderer == null)
        {
            m_Renderer = GetComponent<SkinnedMeshRenderer>();
            if (m_Renderer == null)
            {
                Debug.LogWarningFormat("Renderer not added to the snow occlusion system, there is no " +
                    "MeshRenderer or SkinnedMeshRenderer on {0}.",
                    gameObject.name);
            }
        }
    }

    public override void OnStart()
    {
        Enable();
    }
}
