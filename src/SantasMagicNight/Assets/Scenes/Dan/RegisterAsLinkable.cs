using UnityEngine;

public class RegisterAsLinkable : MonoBehaviour
{
    [SerializeField] private CurrentLevelMap map;

    private void Awake() => map.RegisterAsLinkable(gameObject);
}
