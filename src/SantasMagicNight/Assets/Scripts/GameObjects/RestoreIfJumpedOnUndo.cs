using System.Collections.Generic;
using UnityEngine;

public sealed class RestoreIfJumpedOnUndo : OnMessage<UndoPieceMoved, ObjectDestroyed, LevelReset, PieceJumped>
{
    private readonly Stack<GameObject> _damagedObjects = new Stack<GameObject>();
    
    protected override void Execute(UndoPieceMoved msg)
    {
        if (_damagedObjects.Count == 0)
            return;
        RestoreAllCollectedStars();

        var obj = _damagedObjects.Peek();

        Debug.Log("Undo Object: " + obj.name);
        Debug.Log("Undo Piece: " + msg.Piece);
        Debug.Log("To: " + msg.To);
        Debug.Log("From: " + msg.From);

        var destroyIfJumpedComponent = obj.GetComponent<DestroyIfJumped>();
        if (destroyIfJumpedComponent != null)
        {
            destroyIfJumpedComponent.Revert();
            Message.Publish(new UndoObjectDestroyed(obj));
            _damagedObjects.Pop();
        }

        var destroyIfJumpedAlt = obj.GetComponent<DestroyIfJumpedNoDeathAnim>();
        if (destroyIfJumpedAlt != null)
        {
            destroyIfJumpedAlt.Revert();
            Message.Publish(new UndoObjectDestroyed(obj));
            _damagedObjects.Pop();
        }

        var destroyIfDoubleJumpedComponent = obj.GetComponent<DestroyIfDoubleJumped>();
        if (destroyIfDoubleJumpedComponent != null)
        {
            destroyIfDoubleJumpedComponent.Revert();
            Message.Publish(new UndoObjectDestroyed(obj));
            _damagedObjects.Pop();
        }

        var destroyIfLinked = obj.GetComponent<DestroyIfLinked>();
        if (destroyIfLinked != null)
        {
            destroyIfLinked.Revert();
            Message.Publish(new UndoObjectDestroyed(obj));
            _damagedObjects.Pop();
        }
    }

    private void RestoreAllCollectedStars()
    {
        var obj = _damagedObjects.Peek();
        var collectedStartComponent = obj.GetComponent<CollectStarOnEntered>();
        if (collectedStartComponent != null)
        {
            collectedStartComponent.Revert();
            Message.Publish(new UndoObjectDestroyed(obj));
            _damagedObjects.Pop();
        }
    }

    protected override void Execute(ObjectDestroyed msg)
    {
        //Debug.Log("Pushing to destroyed stack: " + msg.Object.name);
        _damagedObjects.Push(msg.Object);
    }

    protected override void Execute(LevelReset msg) => _damagedObjects.Clear();
    protected override void Execute(PieceJumped msg) => _damagedObjects.Push(msg.Piece);
}

