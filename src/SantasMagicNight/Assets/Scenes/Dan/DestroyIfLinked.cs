using UnityEngine;

public sealed class DestroyIfLinked : OnMessage<PieceMoved>
{    
    [SerializeField] private UiSfxPlayer sfx;
    [SerializeField] private AudioClipWithVolume sound;
    
    protected override void Execute(PieceMoved msg)
    {
        if (!msg.HasSelected(gameObject)) return;
        
        sfx.Play(sound.clip, sound.volume);
        Message.Publish(new ObjectDestroyed(gameObject, false));
        FindObjectOfType<MoveProcessor>().ProcessLinkable(msg);
    }

    public void Revert()
    {
        gameObject.SetActive(true);
    }
}
