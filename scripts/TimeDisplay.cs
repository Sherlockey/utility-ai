using Godot;

using System;

public partial class TimeDisplay : PanelContainer
{
    [Export]
    private Button _pauseButton;
    [Export]
    private Button _1XButton;
    [Export]
    private Button _2XButton;
    [Export]
    private Button _3XButton;

    public override void _Ready()
    {
        _pauseButton.Pressed += OnPauseButtonPressed;
        _1XButton.Pressed += On1XButtonPressed;
        _2XButton.Pressed += On2XButtonPressed;
        _3XButton.Pressed += On3XButtonPressed;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Quoteleft || keyEvent.Keycode == Key.P)
            {
                OnPauseButtonPressed();
            }
            if (keyEvent.Keycode == Key.Key1)
            {
                On1XButtonPressed();
            }
            if (keyEvent.Keycode == Key.Key2)
            {
                On2XButtonPressed();
            }
            if (keyEvent.Keycode == Key.Key3)
            {
                On3XButtonPressed();
            }
        }
    }

    private void OnPauseButtonPressed()
    {
        Engine.TimeScale = 0.0;
    }

    private void On1XButtonPressed()
    {
        Engine.TimeScale = 1.0;
    }

    private void On2XButtonPressed()
    {
        Engine.TimeScale = 2.0;
    }

    private void On3XButtonPressed()
    {
        Engine.TimeScale = 3.0;
    }
}
