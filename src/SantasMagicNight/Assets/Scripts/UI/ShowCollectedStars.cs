using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class ShowCollectedStars : OnMessage<StarCollected, UndoStarCollected, LevelReset>
{
    [SerializeField] private GameObject parent;
    [SerializeField] private GameObject starPrototype;

    private readonly HashSet<string> _collectedStarTypes = new HashSet<string>();
    private readonly List<GameObject> _stars = new List<GameObject>();

    protected override void Execute(StarCollected msg)
    {
        if (_collectedStarTypes.Contains(msg.StarType))
            return;
        
        _collectedStarTypes.Add(msg.StarType);
        _stars.Add(Instantiate(starPrototype, parent.transform));
    }

    protected override void Execute(UndoStarCollected msg)
    {
        if (!_stars.Any() || !_collectedStarTypes.Contains(msg.StarType))
            return;
        
        var star = _stars[0];
        _collectedStarTypes.Remove(msg.StarType);
        _stars.RemoveAt(0);
        Destroy(star);
    }

    protected override void Execute(LevelReset msg)
    {
        _collectedStarTypes.Clear();
        while (_stars.Any())
        {
            var star = _stars[0];
            _stars.RemoveAt(0);
            Destroy(star);
        }
    }
}
