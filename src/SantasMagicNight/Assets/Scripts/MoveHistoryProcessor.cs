using System.Linq;
using UnityEngine;

public sealed class MoveHistoryProcessor : MonoBehaviour
{
    [SerializeField] private MoveHistory history;
    [SerializeField] private CurrentLevelMap map;

    private void OnEnable()
    {
        Message.Subscribe<UndoRequested>(_ => history.Undo(), this);
        Message.Subscribe<LevelReset>(_ => history.Reset(), this);
        Message.Subscribe<PieceMoved>(Execute, this);
    }
    
    private void Execute(PieceMoved msg)
    {
        history.Add(msg);
        if (map.LinkableObjects.Any(l => msg.From.IsAdjacentTo(new TilePoint(l))))
            return;
        if (map.IsPushingTile(msg.From) && !map.IsBlocked(msg.To))
            return;
        history.FinishTurn();
        Message.Publish(new TurnMovementFinished());
    }

    private void OnDisable()
    {
        Message.Unsubscribe(this);
    }
}
