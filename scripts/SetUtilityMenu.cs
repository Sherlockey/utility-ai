using Godot;

using System;

public partial class SetUtilityMenu : PanelContainer
{
    [Export]
    private Label _nameLabel;
    [Export]
    private TextureRect _portrait;
    [Export]
    private Button _combatantLeftButton;
    [Export]
    private Button _combatantRightButton;
    [Export]
    private Control _movementUtilitiesParent;
    [Export]
    private Control _abilityUtilitiesParent;

    private int _partyIndex = 0;

    public override void _Ready()
    {
        if (_partyIndex > Game.Instance.Party.Count - 1)
        {
            return;
        }

        RefreshDisplay();

        _combatantLeftButton.Pressed += OnCombatantLeftButtonPressed;
        _combatantRightButton.Pressed += OnCombatantRightButtonPressed;
    }

    public void RefreshDisplay()
    {
        FreeSetUtilityControls();

        if (_partyIndex > Game.Instance.Party.Count - 1)
        {
            return;
        }
        Combatant combatant = Game.Instance.Party[_partyIndex];
        _nameLabel.Text = combatant.DisplayName;
        _portrait.Texture = combatant.Sprite2D.Texture;

        foreach (MovementUtilityFunction muf in combatant.Brain.MovementUtilities)
        {
            Control control = muf.InstantiateSetUtilityControl();
            _movementUtilitiesParent.AddChild(control);
        }

        foreach (AbilityUtilityFunction auf in combatant.Brain.AbilityUtilities)
        {
            Control control = auf.InstantiateSetUtilityControl();
            _abilityUtilitiesParent.AddChild(control);
        }
    }

    private void FreeSetUtilityControls()
    {
        foreach (Node child in _movementUtilitiesParent.GetChildren())
        {
            child.Free();
        }
        foreach (Node child in _abilityUtilitiesParent.GetChildren())
        {
            child.Free();
        }
    }

    private void OnCombatantLeftButtonPressed()
    {
        int before = _partyIndex;
        _partyIndex--;
        if (_partyIndex < 0)
        {
            _partyIndex = Game.Instance.Party.Count - 1;
        }
        if (_partyIndex != before)
        {
            RefreshDisplay();
        }
    }

    private void OnCombatantRightButtonPressed()
    {
        int before = _partyIndex;
        _partyIndex++;
        if (_partyIndex > Game.Instance.Party.Count - 1)
        {
            _partyIndex = 0;
        }
        if (_partyIndex != before)
        {
            RefreshDisplay();
        }
    }
}
