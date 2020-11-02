
using System;
using UnityEngine;

public sealed class SantaLevelButton : MonoBehaviour
{
    [SerializeField] private TextCommandButton button;
    [SerializeField] private GameObject[] starCounters;

    public void Init(string commandText, int score, Action cmd)
    {
        button.Init(commandText, cmd);
        for(var i = 0; i < starCounters.Length; i++)
            starCounters[i].SetActive(score > i);
    }
}
