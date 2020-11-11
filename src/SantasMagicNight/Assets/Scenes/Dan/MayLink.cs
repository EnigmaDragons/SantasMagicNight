using UnityEngine;

[CreateAssetMenu]
public sealed class MayLink : MovementOptionRule
{
    [SerializeField] private CurrentLevelMap map;

    public override MovementType Type => MovementType.Link;

    public override bool IsPossible(MoveToRequested m) 
        => m.From.IsAdjacentTo(m.To) && map.IsLinkable(m.To);
}
