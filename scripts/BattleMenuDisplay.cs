using Godot;

using System;

public partial class BattleMenuDisplay : PanelContainer
{
    public event EventHandler RanAway;

    [Export]
    private Button _runButton;
    [Export]
    private Button _closeButton;

    public override void _Ready()
    {
        _runButton.Pressed += OnRunButtonPressed;
        _closeButton.Pressed += OnCloseButtonPressed;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                Visible = !Visible;
            }
        }
    }

    private void OnRunButtonPressed()
    {
        Visible = false;
        RanAway?.Invoke(this, EventArgs.Empty); // TODO maybe add a confirmation here first instead
    }

    private void OnCloseButtonPressed()
    {
        Visible = false;
    }
}
