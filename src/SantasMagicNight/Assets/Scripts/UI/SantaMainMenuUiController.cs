using System;
using UnityEngine;

public class SantaMainMenuUiController : OnMessage<ShowOptionsMenu, HideCurrentSubMenu, ShowLevelSelect>
{
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject levelSelect;

    private void HideAllAndThen(Action afterHiding)
    {
        levelSelect.SetActive(false);
        mainMenuRoot.SetActive(false);
        optionsMenu.SetActive(false);
        afterHiding();
    }
    
    protected override void Execute(ShowOptionsMenu msg) 
        => HideAllAndThen(() => optionsMenu.SetActive(true));

    protected override void Execute(HideCurrentSubMenu msg)
        => HideAllAndThen(() => mainMenuRoot.SetActive(true));

    protected override void Execute(ShowLevelSelect msg)
        => HideAllAndThen(() => levelSelect.SetActive(true));
}
