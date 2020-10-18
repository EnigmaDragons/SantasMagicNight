using System;
using UnityEngine;

[Serializable]
public sealed class PieceMoved
{
    public GameObject Piece { get; }
    public TilePoint From { get; }
    public TilePoint To { get; }
    public TilePoint Delta => To - From;

    public PieceMoved(GameObject obj, TilePoint from, TilePoint to)
    {
        Piece = obj;
        From = from;
        To = to;
    }

    public bool HasJumpedOver(GameObject other)
    {
        var hasJumpedOver = From.IsAdjacentTo(new TilePoint(other)) && To.IsAdjacentTo(new TilePoint(other))
                                                       && (To.X == From.X || To.Y == From.Y);
        return hasJumpedOver;
    }

    public bool HasSelected(GameObject other)
    {
        var hasSelected = (To.X == other.transform.localPosition.x && To.Y == other.transform.localPosition.y);
        return hasSelected;
    }

    public void Undo() => Message.Publish(new UndoPieceMoved(Piece, From, To));
}
