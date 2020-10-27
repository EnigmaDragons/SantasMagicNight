using System;
using UnityEngine;
using UnityEngine.UI;

public class InitSantaLevelSelectButtons : MonoBehaviour
{
    [SerializeField] private Campaign campaign;
    [SerializeField] private SantaNavigator navigator;
    [SerializeField] private CurrentLevel level;
    [SerializeField] private TextCommandButton buttonPrototype;
    [SerializeField] private CurrentZone currentZone;
    [SerializeField] private GameObject parent;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    private int _zone = 0;
    
    void Awake()
    {
        if (currentZone.Campaign != campaign)
            currentZone.Init(campaign);
        
        previousButton.onClick.AddListener(MovePrevious);
        nextButton.onClick.AddListener(MoveNext);
        UpdateUi();
    }

    private void UpdateUi()
    {
        foreach(Transform t in parent.transform)
            Destroy(t.gameObject);
        
        var zone = campaign.Value[_zone];
        for (var i = 0; i < zone.Value.Length; i++)
        {
            var currentIndex = i;
            Instantiate(buttonPrototype, parent.transform).Init($"{_zone + 1} - {i + 1}", () => 
            { 
                level.SelectLevel(zone.Value[currentIndex], 0, currentIndex);
                navigator.NavigateToGameScene();
            });
        }

        previousButton.gameObject.SetActive(_zone > 0);
        nextButton.gameObject.SetActive(_zone < campaign.Value.Length - 1);
    }
    
    private void MovePrevious()
    {
        if (_zone == 0)
            return;

        _zone = Math.Max(0, _zone - 1);
        UpdateUi();
    }
    
    private void MoveNext()
    {
        if (_zone == campaign.Value.Length - 1)
            return;

        _zone = Math.Min(campaign.Value.Length - 1, _zone + 1);
        UpdateUi();
    }
}
