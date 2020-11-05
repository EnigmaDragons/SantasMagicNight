using UnityEngine;

public class PlayCurrentZoneMusicOnStart : OnMessage<LevelReset>
{
     [SerializeField] private AudioClip defaultMusic;
     [SerializeField] private CurrentZone currentZone;
     [SerializeField] private GameMusicPlayer player;

     void Start() => Play();
     protected override void Execute(LevelReset msg) => Play();

     private void Play()
     {
          var music = currentZone.Zone.MusicTheme != null ? currentZone.Zone.MusicTheme : defaultMusic;
          player.PlaySelectedMusicLooping(music);
     }
}
