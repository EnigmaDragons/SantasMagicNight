using UnityEngine;

[CreateAssetMenu(menuName = "Rules/Movement/MustMoveToAdjacentTile")]
public class MustMoveToAdjacentTile : MovementRestrictionRule
{
    [SerializeField] private CurrentLevelMap map;

    public override bool IsValid(MovementProposed m) => m.From.IsAdjacentTo(m.To);
}
