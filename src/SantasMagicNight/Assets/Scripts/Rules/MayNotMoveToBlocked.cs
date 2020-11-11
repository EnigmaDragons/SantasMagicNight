using UnityEngine;

[CreateAssetMenu(menuName = "Rules/Movement/MayNotMoveToBlocked")]
public sealed class MayNotMoveToBlocked : MovementRestrictionRule
{
    [SerializeField] private CurrentLevelMap map;

    public override bool IsValid(MovementProposed m) => m.Type == MovementType.Attack 
                                                        || m.Type == MovementType.Link 
                                                        || !map.IsBlocked(m.To);
}
