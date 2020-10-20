using UnityEngine;

public class SantaMainMenuUiController : OnMessage<ShowOptionsMenu, HideOptionsMenu>
{
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject optionsMenu;
    
    protected override void Execute(ShowOptionsMenu msg)
    {
        mainMenuRoot.SetActive(false);
        optionsMenu.SetActive(true);
    }

    protected override void Execute(HideOptionsMenu msg)
    {
        optionsMenu.SetActive(false);
        mainMenuRoot.SetActive(true);
    }
}
