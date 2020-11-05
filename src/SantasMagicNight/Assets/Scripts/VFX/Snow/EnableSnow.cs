
using UnityEngine;

public class EnableSnow : MonoBehaviour
{
    private void Start() => Message.Publish(new EnableSnowRequested());
}
