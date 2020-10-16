using UnityEngine;

[CreateAssetMenu(menuName = "Rules/Movement/MustMoveToWalkable")]
public class MustMoveToWalkable : MovementRestrictionRule
{
    [SerializeField] private CurrentLevelMap map;

    public override bool IsValid(MovementProposed m) => m.Type == MovementType.Attack || map.IsWalkable(m.To);
}
