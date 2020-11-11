using UnityEngine;

[CreateAssetMenu(menuName = "Rules/Movement/MayNotMoveWhileBeingPushed")]
public sealed class MayNotMoveWhileBeingPushed : MovementRestrictionRule
{
    [SerializeField] private CurrentLevelMap map;

    public override bool IsValid(MovementProposed m) => !map.IsPieceBeingPushed(m.Piece);
}

