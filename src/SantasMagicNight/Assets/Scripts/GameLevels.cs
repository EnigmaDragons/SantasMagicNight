using System;
using System.Collections.Generic;
using E7.Introloop;
using UnityEngine;

[CreateAssetMenu]
public sealed class GameLevels : ScriptableObject
{
    [SerializeField] private GameLevel[] value;
    [SerializeField] private IntReference starsRequired;
    [SerializeField] private UnityDateTimeOffset minDateRequired = DateTimeOffset.MinValue.AddDays(2); 
    [SerializeField] private string name;
    [SerializeField] private string description;
    [SerializeField] private Sprite logo;
    [SerializeField] private Sprite logoTiled;
    [SerializeField] private Color logoColor;
    [SerializeField] private Color backgroundColor;
    [SerializeField] private AudioClip musicTheme;
    [SerializeField] private SaveStorage saveStorage;
    [SerializeField] private GameLevel tutorial;
    [SerializeField] private int[] progression;

    public GameLevel[] Value => value;
    public int StarsRequired => starsRequired;
    public DateTimeOffset MinDateRequired => minDateRequired;
    public string Name => name;
    public string Description => description;
    public Sprite Logo => logo;
    public Sprite LogoTiled => logoTiled;
    public Color LogoColor => logoColor;
    public Color BackgroundColor => backgroundColor;
    public AudioClip MusicTheme => musicTheme;
    public Maybe<GameLevel> Tutorial => tutorial;
    public int[] Progression => progression;
}
