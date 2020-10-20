using UnityEngine;

public class RegisterAsPushingTile : MonoBehaviour
{
    [SerializeField] private CurrentLevelMap currentLevelMap;

    private void Awake() => currentLevelMap.RegisterPushingTile(gameObject);
}
