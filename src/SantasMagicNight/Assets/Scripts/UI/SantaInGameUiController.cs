
using UnityEngine;

public class SantaInGameUiController : OnMessage<ShowOptionsMenu, HideCurrentSubMenu>
{
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject inGameControls;

    private void Awake()
    {
        Time.timeScale = 1;
        inGameControls.SetActive(true);
        optionsMenu.SetActive(false);
    }
    
    protected override void Execute(ShowOptionsMenu msg)
    {
        inGameControls.SetActive(false);
        Time.timeScale = 0;
        optionsMenu.SetActive(true);
    }

    protected override void Execute(HideCurrentSubMenu msg)
    {
        optionsMenu.SetActive(false);
        Time.timeScale = 1;
        inGameControls.SetActive(true);
    }
}
