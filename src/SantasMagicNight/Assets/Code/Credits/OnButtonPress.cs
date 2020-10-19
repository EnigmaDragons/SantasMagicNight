using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class OnButtonPress : MonoBehaviour
{
    [SerializeField] private string[] buttonNames;
    [SerializeField] private UnityEvent action;

    private bool _triggered;
    
    void Update()
    {
        if (!_triggered && buttonNames.Any(Input.GetButton))
        {
            _triggered = true;
            action.Invoke();
        }
    }
}
