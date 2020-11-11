using UnityEngine;

public sealed class PieceSelectionIndicator : OnMessage<PieceSelected, PieceDeselected, PlayerPrefsChanged>
{
    [SerializeField] private GameObject indicator;
    [SerializeField] private GameObject canSelectIndicator;
    [SerializeField] private StringVariable enabledOptionKey;

    private bool _isSelected;
    private bool HintsEnabled 
        => !PlayerPrefs.HasKey(enabledOptionKey)
           || PlayerPrefs.GetInt(enabledOptionKey) != 0;
    
    private void Start() => UpdateSelectors(false);
    protected override void Execute(PieceDeselected msg) => UpdateSelectors(false);
    protected override void Execute(PieceSelected msg) => UpdateSelectors(msg.Piece.Equals(transform.gameObject));
    protected override void Execute(PlayerPrefsChanged msg) => UpdateSelectors(_isSelected);

    private void UpdateSelectors(bool isSelected)
    {
        _isSelected = isSelected;
        indicator.SetActive(_isSelected);
        if (canSelectIndicator != null) 
            canSelectIndicator.SetActive(HintsEnabled && !_isSelected);
    }
}
