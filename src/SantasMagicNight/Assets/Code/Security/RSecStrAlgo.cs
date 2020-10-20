using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Security/RSecStrAlgo")]
public sealed class RSecStrAlgo : StringSecurityAlgorithm
{
    public override string GetValue(string secureString) 
        => new String(secureString.Reverse().ToArray());
}
