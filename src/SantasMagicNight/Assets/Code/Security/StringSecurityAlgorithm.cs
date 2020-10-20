using UnityEngine;

public abstract class StringSecurityAlgorithm : ScriptableObject
{
    public abstract string GetValue(string secureString);
}
