
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BindButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private UnityEvent action;

    private void Awake() => button.onClick.AddListener(() => action.Invoke());
}
