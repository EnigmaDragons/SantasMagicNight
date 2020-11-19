using UnityEngine;
using UnityEngine.Events;

public class ExecuteLifecycleActions : MonoBehaviour
{
    [SerializeField] private UnityEvent onAwake;
    [SerializeField] private UnityEvent onEnable;
    [SerializeField] private UnityEvent onDisable;
    [SerializeField] private UnityEvent onStart;

    private void Awake() => onAwake?.Invoke();
    private void OnEnable() => onEnable?.Invoke();
    private void OnDisable() => onDisable?.Invoke();
    private void Start() => onStart?.Invoke();
}
