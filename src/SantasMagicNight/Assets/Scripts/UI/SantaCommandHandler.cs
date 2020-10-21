using UnityEngine;

public class SantaCommandHandler : OnMessage<RetryLevel, GoToNextLevel>
{
    [SerializeField] private SantaNavigator navigator;
    [SerializeField] private GameLevels levels;
    [SerializeField] private CurrentLevel currentLevel;
    [SerializeField] private SaveStorage saveStorage;
    [SerializeField] private StarCounter counter;

    protected override void Execute(RetryLevel msg)
    {
        saveStorage.SaveStars(currentLevel.ActiveLevel, counter.NumStars);
        navigator.NavigateToGameScene();
    }

    protected override void Execute(GoToNextLevel msg)
    {
        var nextLevelNumber = currentLevel.LevelNumber + 1;
        saveStorage.SaveStars(currentLevel.ActiveLevel, counter.NumStars);
        
        if (levels.Value.Length > nextLevelNumber)
        {
            currentLevel.SelectLevel(levels.Value[nextLevelNumber], 0, nextLevelNumber);
            navigator.NavigateToGameScene();
        }
        else
        {
            navigator.NavigateToCredits();
        }
    }
}
