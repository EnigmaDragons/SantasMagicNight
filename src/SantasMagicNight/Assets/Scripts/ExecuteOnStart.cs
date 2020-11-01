using UnityEngine;
using UnityEngine.Events;

public sealed class ExecuteOnStart : MonoBehaviour
{
    [SerializeField] private UnityEvent action;

    private void Start() => action.Invoke();
}
