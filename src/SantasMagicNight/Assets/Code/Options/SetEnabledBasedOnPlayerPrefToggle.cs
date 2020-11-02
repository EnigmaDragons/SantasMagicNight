using UnityEngine;

public sealed class SetEnabledBasedOnPlayerPrefToggle : OnMessage<PlayerPrefsChanged>
{
    [SerializeField] private StringReference key;
    [SerializeField] private GameObject target;

    private void Start() => UpdateActive();
    protected override void Execute(PlayerPrefsChanged msg) => UpdateActive();

    private void UpdateActive()
    {
        target.SetActive(PlayerPrefs.HasKey(key) && PlayerPrefs.GetInt(key) != 0);
    }
}
