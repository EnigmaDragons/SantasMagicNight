using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public sealed class LerpTransform : MonoBehaviour
{
    [SerializeField] private bool startOnAwake;
    [SerializeField] private float delaySecondsBeforeStartOnAwake;
    [SerializeField] private Transform initial;
    [SerializeField] private Transform final;
    [SerializeField] private float durationSeconds;
    [SerializeField] private UnityEvent onFinished;
    
    private bool _isFinished = true;
    private float _currentDuration;
    private Vector3 _positionVelocity = Vector3.zero;
    private Vector3 _rotationVelocity = Vector3.zero;

    private void Awake()
    {
        if (startOnAwake)
            StartCoroutine(ExecuteAfterDelay(StartAnim, delaySecondsBeforeStartOnAwake));

        transform.localRotation = initial.localRotation;
        transform.localPosition = initial.localPosition;
        transform.localScale = initial.localScale;
    }
    
    void LateUpdate()
    {
        if (_isFinished)
            return;

        _currentDuration = Mathf.Min(durationSeconds, _currentDuration + Time.deltaTime);
        var amount = _currentDuration / durationSeconds;
        var smoothedAmount = amount * amount * (3f - 2f * amount);
        //var rate = Mathf.Pow(Mathf.Log(1.1f), 2);
        //var amount = Mathf.Pow(-rate * Time.deltaTime, 2);

        Debug.Log($"Delta: {Time.deltaTime} Amount: {amount}, Elapsed: {_currentDuration}, Target: {durationSeconds}");
        transform.localPosition = Vector3.Slerp(initial.localPosition, final.localPosition, smoothedAmount);
        transform.localRotation = Quaternion.Slerp(initial.localRotation, final.localRotation, smoothedAmount);
        transform.localScale = Vector3.Slerp(initial.localScale, final.localScale, smoothedAmount);

        _isFinished = _currentDuration >= durationSeconds;
        if (_isFinished)
            onFinished.Invoke();
    }
    
    
    public void StartAnim()
    {
        _isFinished = false;
        _currentDuration = 0;
    }

    private IEnumerator ExecuteAfterDelay(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }
}
