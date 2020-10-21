
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ExecuteAfterDelay : MonoBehaviour
{
    [SerializeField] private float delaySeconds;
    [SerializeField] private UnityEvent action;

    private void Awake()
    {
        StartCoroutine(AfterDelay(() => action.Invoke(), delaySeconds));
    }
    
    private IEnumerator AfterDelay(Action a, float delay)
    {
        yield return new WaitForSeconds(delay);
        a();
    }
}
