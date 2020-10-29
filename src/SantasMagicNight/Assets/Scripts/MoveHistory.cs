using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "State/MoveHistory")]
public sealed class MoveHistory : ScriptableObject
{
    [SerializeField] private GameEvent onChanged;
    [SerializeField] private IntReference maxUndoDepth;

    private FixedSizeStack<List<PieceMoved>> _turns;
    private List<PieceMoved> _currentTurn = new List<PieceMoved>();
    private FixedSizeStack<List<PieceMoved>> Turns
    {
        get
        {
            if (_turns == null || _turns.Size < maxUndoDepth)
                _turns = new FixedSizeStack<List<PieceMoved>>(maxUndoDepth);
            return _turns;
        }
    }

    public GameEvent OnChanged => onChanged;
    public void Reset() => Notify(() =>
    {
        Turns.Clear();
        _currentTurn.Clear();
    });

    public void Add(PieceMoved p) =>_currentTurn.Add(p);
    public void FinishTurn() => Notify(() =>
    {
        Turns.Push(_currentTurn);
        Debug.Log($"Finished Turn {Turns.Count()} with {_currentTurn.Count()} Steps");
        _currentTurn = new List<PieceMoved>();
    });
    
    public void Undo()
    {
        if (Turns.Count() > 0)
            Notify(() => Turns.Pop().ForEach(m => m.Undo()));
    }

    public int Count => Turns.Count();

    private void Notify(Action a)
    {
        a();
        onChanged.Publish();
    }
}
