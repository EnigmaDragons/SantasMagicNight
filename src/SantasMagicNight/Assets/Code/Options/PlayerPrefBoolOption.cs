using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerPrefBoolOption : MonoBehaviour
{
    [SerializeField] private StringReference key;
    [SerializeField] private Toggle toggle;

    private void Awake()
    {
        if (toggle != null) 
            toggle.onValueChanged.AddListener(Set);
    }

    private void OnEnable()
    {
        if (toggle != null)
            toggle.SetIsOnWithoutNotify(IsEnabled);
    }

    public bool IsEnabled 
        => PlayerPrefs.HasKey(key)
           && PlayerPrefs.GetInt(key) != 0;

    public void Set(bool isEnabled)
    {
        PlayerPrefs.SetInt(key, isEnabled ? 1 : 0);
        PlayerPrefs.Save();
        Message.Publish(new PlayerPrefsChanged());
    }

    public void Toggle() => Set(!IsEnabled);
}
