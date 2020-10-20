using UnityEngine;

public class InitSantaLevelSelectButtons : MonoBehaviour
{
    [SerializeField] private Campaign campaign;
    [SerializeField] private SantaNavigator navigator;
    [SerializeField] private CurrentLevel level;
    [SerializeField] private TextCommandButton buttonPrototype;
    [SerializeField] private GameObject parent;

    private int _zone = 0;
    
    void Awake()
    {
        foreach(Transform t in parent.transform)
            Destroy(t.gameObject);
        var zone = campaign.Value[_zone];
        for (var i = 0; i < zone.Value.Length; i++)
        {
            var currentIndex = i;
            Instantiate(buttonPrototype, parent.transform).Init($"Level {i + 1}", () => 
            { 
                level.SelectLevel(zone.Value[currentIndex], 0, currentIndex);
                navigator.NavigateToGameScene();
            });
        }
    }
}
