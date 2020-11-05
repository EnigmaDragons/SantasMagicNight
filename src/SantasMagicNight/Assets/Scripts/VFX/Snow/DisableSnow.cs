
using UnityEngine;

public class DisableSnow : MonoBehaviour
{
    private void Start() => Message.Publish(new DisableSnowRequested());
}
