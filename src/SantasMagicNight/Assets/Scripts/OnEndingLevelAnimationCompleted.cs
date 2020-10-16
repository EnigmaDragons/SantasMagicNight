using UnityEngine;

public class OnEndingLevelAnimationCompleted : OnMessage<EndingLevelAnimationFinished>
{
    [SerializeField] private Navigator navigator;
    [SerializeField] private BoolVariable isLevelStart;

    protected override void Execute(EndingLevelAnimationFinished msg)
    {
        isLevelStart.Value = false;
        navigator.NavigateToRewards();
    }
}
