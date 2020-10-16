﻿
using UnityEngine;

[CreateAssetMenu(menuName = "Rules/Movement/MayLeap")]
public class MayLeap : MovementOptionRule
{
    public override MovementType Type => MovementType.Leap;
    
    public override bool IsPossible(MoveToRequested m) => m.Delta.IsCardinal() && m.Delta.TotalMagnitude() == 3;
}
