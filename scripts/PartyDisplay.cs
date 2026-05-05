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
    private BuyUtilityMenu _buyUtilityMenu;
    [Export]
    private Button _buyButton;
    [Export]
    private Button _setButton;
    [Export]
    private Button _closeButton;

    private Control _activeControl;
    private Control _previousControl;

    public override void _Ready()
    {
        _activeControl = _utilityFunctionsMenu;

        _buyButton.Pressed += OnBuyButtonPressed;
        _setButton.Pressed += OnSetButtonPressed;
        _closeButton.Pressed += OnCloseButtonPressed;
    }

    private void OnBuyButtonPressed()
    {
        _utilityFunctionsMenu.Visible = false;
        _buyUtilityMenu.Visible = true;
        _buyUtilityMenu.RefreshDisplay();
        _previousControl = _utilityFunctionsMenu;
        _activeControl = _buyUtilityMenu;
        _closeButton.Text = "Back";
    }

    private void OnSetButtonPressed()
    {
        _utilityFunctionsMenu.Visible = false;
        _setUtilityMenu.Visible = true;
        _setUtilityMenu.RefreshDisplay();
        _previousControl = _utilityFunctionsMenu;
        _activeControl = _setUtilityMenu;
        _closeButton.Text = "Back";
    }

    private void OnCloseButtonPressed()
    {
        if (_previousControl == null)
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            _activeControl.Visible = false;
            _previousControl.Visible = true;
            _activeControl = _previousControl;
            _previousControl = null;
            _closeButton.Text = "Close";
        }
    }
}
