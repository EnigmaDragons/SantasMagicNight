using System.Collections;
using UnityEngine;

public sealed class DestroyIfLinked : OnMessage<PieceMoved>
{
    protected override void Execute(PieceMoved msg)
    {
        if (!msg.HasSelected(gameObject)) return;
        Message.Publish(new ObjectDestroyed(gameObject, false));
        FindObjectOfType<MoveProcessor>().ProcessLinkable(msg);
    }

    public void Revert()
    {
        gameObject.SetActive(true);
    }
}
