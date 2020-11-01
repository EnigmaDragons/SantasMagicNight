using System.Collections.Generic;
using UnityEngine;

public sealed class RestoreOnUndo : OnMessage<UndoPieceMoved, ObjectDestroyed, LevelReset, PieceJumped, TurnMovementFinished>
{
    private readonly Stack<List<GameObject>> _damagedObjects = new Stack<List<GameObject>>();
    private List<GameObject> _lastTurnDestroyedObjects = new List<GameObject>();
    
    protected override void Execute(UndoPieceMoved msg)
    {
        if (_damagedObjects.Count == 0)
            return;

        foreach (var obj in _lastTurnDestroyedObjects)
        {   
            var collectedStartComponent = obj.GetComponent<CollectStarOnEntered>();
            if (collectedStartComponent != null)
            {
                collectedStartComponent.Revert();
                Message.Publish(new UndoObjectDestroyed(obj));
            }
            
            var destroyIfJumpedComponent = obj.GetComponent<DestroyIfJumped>();
            if (destroyIfJumpedComponent != null)
            {
                destroyIfJumpedComponent.Revert();
                Message.Publish(new UndoObjectDestroyed(obj));
            }

            var destroyIfJumpedAlt = obj.GetComponent<DestroyIfJumpedNoDeathAnim>();
            if (destroyIfJumpedAlt != null)
            {
                destroyIfJumpedAlt.Revert();
                Message.Publish(new UndoObjectDestroyed(obj));
            }

            var destroyIfDoubleJumpedComponent = obj.GetComponent<DestroyIfDoubleJumped>();
            if (destroyIfDoubleJumpedComponent != null)
            {
                destroyIfDoubleJumpedComponent.Revert();
                Message.Publish(new UndoObjectDestroyed(obj));
            }

            var destroyIfLinked = obj.GetComponent<DestroyIfLinked>();
            if (destroyIfLinked != null)
            {
                destroyIfLinked.Revert();
                Message.Publish(new UndoObjectDestroyed(obj));
            }
        }

        _lastTurnDestroyedObjects = _damagedObjects.Pop();
    }

    protected override void Execute(ObjectDestroyed msg)
    {
        Debug.Log("Undo - Adding Destroyed " + msg.Object.name);
        _lastTurnDestroyedObjects.Add(msg.Object);
    }

    protected override void Execute(LevelReset msg) => _damagedObjects.Clear();
    protected override void Execute(PieceJumped msg)
    {
        Debug.Log("Undo - Adding Destroyed " + msg.Piece.name);
        _lastTurnDestroyedObjects.Add(msg.Piece);
    }

    protected override void Execute(TurnMovementFinished msg)
    {
        _damagedObjects.Push(_lastTurnDestroyedObjects);
        _lastTurnDestroyedObjects = new List<GameObject>();
        Debug.Log($"Undo - Num Turns: {_damagedObjects.Count}");
    }
}
