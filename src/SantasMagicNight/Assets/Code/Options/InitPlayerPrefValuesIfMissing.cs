using UnityEngine;

public class InitPlayerPrefValuesIfMissing : MonoBehaviour
{
    [SerializeField] private PlayerPrefValue[] values;

    private void Awake()
    {
        values.ForEach(v =>
        {
            if (!PlayerPrefs.HasKey(v.Key))
                PlayerPrefs.SetInt(v.Key, v.IntValue);
        });
        PlayerPrefs.Save();
        Message.Publish(new PlayerPrefsChanged());
    }
}
