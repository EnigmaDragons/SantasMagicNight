using UnityEngine;

[CreateAssetMenu(menuName = "Rules/Movement/MayJump")]
public sealed class MayJump : MovementOptionRule
{
    [SerializeField] private CurrentLevelMap map;

    public override MovementType Type => MovementType.Jump;

    public override bool IsPossible(MoveToRequested m) => m.To.InBetween(m.From).Count == 1 && map.IsJumpable(m.To.InBetween(m.From)[0]);
}
