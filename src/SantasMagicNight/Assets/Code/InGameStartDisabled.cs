using UnityEngine;

public class InGameStartDisabled : MonoBehaviour
{
    [SerializeField] private GameObject target;

    void Awake() => target.SetActive(false);
}
