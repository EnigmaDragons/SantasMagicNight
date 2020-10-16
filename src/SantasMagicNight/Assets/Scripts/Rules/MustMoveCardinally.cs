using UnityEngine;

[CreateAssetMenu(menuName = "Rules/Movement/Must Move Cardinally")]
public sealed class MustMoveCardinally : MovementRestrictionRule
{
    public override bool IsValid(MovementProposed m) => m.Delta.IsCardinal();
}
