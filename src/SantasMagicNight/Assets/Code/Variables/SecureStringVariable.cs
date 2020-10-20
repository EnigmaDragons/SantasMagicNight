using UnityEngine;

[CreateAssetMenu(menuName = "Security/SecureString")]
public sealed class SecureStringVariable : ScriptableObject
{
    [SerializeField] private string value = "";
    [SerializeField] private StringSecurityAlgorithm algorithm;

    public string Value => algorithm.GetValue(value);
    public static implicit operator string(SecureStringVariable s) => s.Value;
}
