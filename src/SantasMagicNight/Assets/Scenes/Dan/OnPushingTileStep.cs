using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class OnPushingTileStep : OnMessage<PieceMoved>
{
    [SerializeField] Vector2 direction = Vector2.up;

    protected override void Execute(PieceMoved msg)
    {
        if (!msg.HasSelected(gameObject)) return;
        Debug.Log("Stepped On: " + gameObject.name);
        Message.Publish(new PieceMoved(msg.Piece, msg.To, msg.To + new TilePoint(direction)));
    }
}
