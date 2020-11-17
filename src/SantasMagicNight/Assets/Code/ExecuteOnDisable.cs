using UnityEngine;
using UnityEngine.Events;

public class ExecuteOnDisable : MonoBehaviour
{
    [SerializeField] private UnityEvent action;

    private void OnDisable() => action.Invoke();
}
