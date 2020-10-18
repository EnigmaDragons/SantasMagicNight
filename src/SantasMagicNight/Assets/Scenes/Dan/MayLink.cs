using System;
using UnityEngine;

[CreateAssetMenu]
public sealed class MayLink : MovementOptionRule
{
    [SerializeField] private CurrentLevelMap map;

    public override MovementType Type => MovementType.Link;

    public override bool IsPossible(MoveToRequested m) => map.IsLinkable(m.To);
}
