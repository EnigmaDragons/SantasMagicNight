using UnityEngine;

public class InvisibleInGame : MonoBehaviour
{
    private void Awake() => GetComponent<Renderer>().enabled = false;
}
