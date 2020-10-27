using UnityEngine;

public class SantaCommandHandler : OnMessage<RetryLevel, GoToNextLevel>
{
    [SerializeField] private SantaNavigator navigator;
    [SerializeField] private CurrentZone levels;
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
        var nextZoneIndex = levels.ZoneIndex + 1;
        saveStorage.SaveStars(currentLevel.ActiveLevel, counter.NumStars);

        var currentZone = levels.Zone;
        var hasNextLevelInZone = currentZone.Value.Length > nextLevelNumber;
        var hasNextZone = levels.Campaign.Value.Length > nextZoneIndex;
        if (hasNextLevelInZone)
        {
            currentLevel.SelectLevel(currentZone.Value[nextLevelNumber], levels.ZoneIndex, nextLevelNumber);
            navigator.NavigateToGameScene();
        }
        else if (hasNextZone)
        {
            currentLevel.SelectLevel(levels.Campaign.Value[nextZoneIndex].Value[0], nextZoneIndex, 0);
            navigator.NavigateToGameScene();
        }
        else
        {
            navigator.NavigateToCredits();
        }
    }
}
