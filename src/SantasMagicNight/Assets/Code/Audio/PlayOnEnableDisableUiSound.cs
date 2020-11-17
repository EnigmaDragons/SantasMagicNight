
using UnityEngine;

public class PlayOnEnableDisableUiSound : MonoBehaviour
{
    [SerializeField] private UiSfxPlayer player;
    [SerializeField] private AudioClipWithVolume onEnableSound;
    [SerializeField] private AudioClipWithVolume onDisableSound;

    private void OnEnable()
    {
        if (onEnableSound != null)
            player.Play(onEnableSound);
    }

    private void OnDisable()
    {
        if (onDisableSound != null)
            player.Play(onDisableSound);
    }
}
