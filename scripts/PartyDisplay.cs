using Godot;

using System;

public partial class PartyDisplay : Control
{
    public event EventHandler Closed;

    [Export]
    private PanelContainer _utilityFunctionsMenu;
    [Export]
    private SetUtilityMenu _setUtilityMenu;
    [Export]
    private Button _buyButton;
    [Export]
    private Button _setButton;
    [Export]
    private Button _closeButton;

    public override void _Ready()
    {
        _buyButton.Pressed += OnBuyButtonPressed;
        _setButton.Pressed += OnSetButtonPressed;
        _closeButton.Pressed += OnCloseButtonPressed;
    }

    private void OnBuyButtonPressed()
    {
        GD.Print("OnBuyButtonPressed Not implemented yet!");
    }

    // TODO WIP
    private void OnSetButtonPressed()
    {
        _utilityFunctionsMenu.Visible = false;
        _setUtilityMenu.Visible = true;
        _setUtilityMenu.RefreshDisplay();
    }

    private void OnCloseButtonPressed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }
}
