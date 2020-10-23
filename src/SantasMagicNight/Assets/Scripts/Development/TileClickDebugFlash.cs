using System.Collections;
using UnityEngine;

public class TileClickDebugFlash : OnMessage<TileIndicated>
{
    [SerializeField] private BoolReference debuggingEnabled;
    [SerializeField] private Renderer target;
    
    protected override void Execute(TileIndicated msg)
    {
#if UNITY_EDITOR        
        if (debuggingEnabled && msg.Tile.Equals(new TilePoint(gameObject)))
            StartCoroutine(Flash());
#endif
    }

    private IEnumerator Flash()
    {
        target.enabled = true;
        yield return new WaitForSeconds(0.5f);
        target.enabled = false;
    }
}
