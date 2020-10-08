using UnityEngine;

public class OnEndingLevelAnimationCompleted : OnMessage<EndingLevelAnimationFinished>
{
    [SerializeField] private Navigator navigator;
    [SerializeField] private BoolVariable isLevelStart;
    [SerializeField] private BoolReference AutoSkipStory;
    [SerializeField] private CurrentDialogue dialogue;

    protected override void Execute(EndingLevelAnimationFinished msg)
    {
        isLevelStart.Value = false;
        if (!AutoSkipStory.Value && dialogue.Dialogue.IsPresent)
            navigator.NavigateToDialogue();
        else
            navigator.NavigateToRewards();
    }
}
