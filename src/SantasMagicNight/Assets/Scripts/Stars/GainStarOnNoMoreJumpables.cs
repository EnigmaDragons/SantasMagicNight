using UnityEngine;

public class GainStarOnNoMoreJumpables : OnMessage<LevelStateChanged, UndoStarCollected>
{
    [SerializeField] private CurrentLevelMap map;

    private bool _awardedStar = false;

    protected override void Execute(LevelStateChanged msg)
    {
        if (!_awardedStar && map.NumOfJumpables == 0)
        {
            _awardedStar = true;
            Message.Publish(StarCollected.NoMoreJumpables);
        }
    }

    protected override void Execute(UndoStarCollected msg)
    {
        if (msg.StarType.Equals(StarCollected.NoMoreJumpables.StarType))
            _awardedStar = false;
    }
}
