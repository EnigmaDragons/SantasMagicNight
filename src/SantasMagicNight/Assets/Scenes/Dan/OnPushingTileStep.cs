using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class OnPushingTileStep : OnMessage<PieceMoved>
{
    [SerializeField] Vector2 direction = Vector2.up;
    [SerializeField] CurrentLevelMap currentLevelMap;

    protected override void Execute(PieceMoved msg)
    {
        if (!msg.HasSelected(gameObject)) return;
        if (currentLevelMap.IsBlocked(msg.To + new TilePoint(direction)))
        {
            Debug.Log((msg.To + new TilePoint(direction)) + " IS BLOCKED!!");
            return;
        }
        StartCoroutine(PushPiece(msg));
    }

    IEnumerator PushPiece(PieceMoved msg)
    {
        Debug.Log("From: " + msg.From +  " To: " + msg.To);
        yield return new WaitForSeconds(1f);
        Message.Publish(new PieceMoved(msg.Piece, msg.To, msg.To + new TilePoint(direction)));
        Debug.Log("Pushing Tile Action Ended");
    }
}
