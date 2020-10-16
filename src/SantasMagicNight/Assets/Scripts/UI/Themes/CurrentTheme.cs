using System;
using UnityEngine;

[Obsolete, CreateAssetMenu(menuName = "Theme/Current Theme")]
public sealed class CurrentTheme : ScriptableObject
{
    [SerializeField] private Theme theme;

    public Theme Current => theme;
    public void Set(Theme v) => theme = v;

    public Color ColorFor(ThemeElement e) => theme.ColorFor(e);
}
