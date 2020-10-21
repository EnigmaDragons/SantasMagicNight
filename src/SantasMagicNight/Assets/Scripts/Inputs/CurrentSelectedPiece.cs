using UnityEngine;

[CreateAssetMenu(menuName = "State/CurrentSelectedPiece")]
public class CurrentSelectedPiece : ScriptableObject
{
    [SerializeField] private Maybe<GameObject> selected = new Maybe<GameObject>();
    [SerializeField] private GameEvent onChange;
    [SerializeField] private GameObject displayObj;

    public GameEvent OnChanged => onChange;
    public Maybe<GameObject> Selected => selected;

    public void Select(GameObject obj)
    {
        Debug.Log($"Selected {obj.name}");
        selected = obj;
        displayObj = obj;
        onChange.Publish();
    }

    public void Deselect()
    {
        // In case Maybe was previously Present, but Unity Lifecycle has expired
        if (selected.IsPresent && selected.Value != null) 
            Debug.Log($"Deselected {selected.Value.name}");
        
        selected = new Maybe<GameObject>();
        displayObj = null;
        onChange.Publish();
    }
}
