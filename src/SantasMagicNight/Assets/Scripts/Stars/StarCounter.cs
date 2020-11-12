using System.Collections.Generic;

public sealed class StarCounter : OnMessage<StarCollected, UndoStarCollected, LevelReset>
{
    private readonly HashSet<string> _starTypes = new HashSet<string>();
    
    public int NumStars => _starTypes.Count;

    protected override void Execute(StarCollected msg) => _starTypes.Add(msg.StarType);
    protected override void Execute(UndoStarCollected msg) => _starTypes.Remove(msg.StarType);
    protected override void Execute(LevelReset msg) => _starTypes.Clear();
}
