using TMPro;
using UnityEngine;

public class InitLevelInfo : MonoBehaviour
{
    [SerializeField] private BoolReference isLevelStart;
    [SerializeField] private TextMeshProUGUI label;

    private void Awake()
    {
        label.text = $"{name}";
    }
}
