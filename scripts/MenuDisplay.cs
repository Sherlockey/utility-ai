using Godot;

using System;

public partial class MenuDisplay : Control
{
    [Export]
    private PanelContainer _menuList;
    [Export]
    private EmbarkDisplay _embarkDisplay;
    [Export]
    private Button _partyButton;
    [Export]
    private Button _embarkButton;

    [Export]
    private PartyDisplay _partyDisplay;

    public override void _Ready()
    {
        _partyButton.Pressed += OnPartyButtonPressed;
        _embarkButton.Pressed += OnEmbarkButtonPressed;

        _partyDisplay.Closed += OnPartyDisplayClosed;
        _embarkDisplay.Closed += OnEmbarkDisplayClosed;
    }

    private void OnPartyButtonPressed()
    {
        _menuList.Visible = false;
        _partyDisplay.Visible = true;
    }

    private void OnEmbarkButtonPressed()
    {
        _menuList.Visible = false;
        _embarkDisplay.Visible = true;
    }

    private void OnPartyDisplayClosed(object sender, EventArgs e)
    {
        _partyDisplay.Visible = false;
        _menuList.Visible = true;
    }

    private void OnEmbarkDisplayClosed(object sender, EventArgs e)
    {
        _embarkDisplay.Visible = false;
        _menuList.Visible = true;
    }
}
