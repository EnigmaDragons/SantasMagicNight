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
        if (map.IsPushing(msg.From))
            return;
        history.FinishTurn();
    }

    private void OnDisable()
    {
        Message.Unsubscribe(this);
    }
}
