using UnityEngine;

public class SantaInGameUiController : OnMessage<ShowOptionsMenu, ShowLevelSelect, HideCurrentSubMenu>
{
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject inGameControls;
    [SerializeField] private GameObject levelSelect;

    private void Awake()
    {
        Time.timeScale = 1;
        inGameControls.SetActive(true);
        optionsMenu.SetActive(false);
        levelSelect.SetActive(false);
    }
    
    protected override void Execute(ShowOptionsMenu msg)
    {
        HideAll();
        Time.timeScale = 0;
        optionsMenu.SetActive(true);
    }

    protected override void Execute(ShowLevelSelect msg)
    {
        HideAll();
        Time.timeScale = 0;
        levelSelect.SetActive(true);
    }

    protected override void Execute(HideCurrentSubMenu msg)
    {
        HideAll();
        Time.timeScale = 1;
        inGameControls.SetActive(true);
    }

    private void HideAll()
    {
        optionsMenu.SetActive(false);
        inGameControls.SetActive(false);
        levelSelect.SetActive(false);
    }
}
