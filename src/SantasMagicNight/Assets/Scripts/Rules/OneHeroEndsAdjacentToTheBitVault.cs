using UnityEngine;

[CreateAssetMenu(menuName = "Rules/Victory/OneHeroEndsAdjacentToTheBitVault")]
public sealed class OneHeroEndsAdjacentToTheBitVault : VictoryCondition
{
    public override bool HasCompletedLevel(CurrentLevelMap map)
        => new TilePoint(map.Hero).IsAdjacentTo(map.BitVaultLocation);
}
