
using UnityEngine;
using UnityEngine.Events;

public class BindTextCommandButton : MonoBehaviour
{
    [SerializeField] private TextCommandButton button;
    [SerializeField] private string text;
    [SerializeField] private UnityEvent action;

    private void Awake() => button.Init(text, () => action.Invoke());
}
