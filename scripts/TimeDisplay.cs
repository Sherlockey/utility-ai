using Godot;

using System;

public partial class TimeDisplay : PanelContainer
{
    public event EventHandler XButtonPressed;

    [Export]
    private Button _pauseButton;
    [Export]
    private Button _1XButton;
    [Export]
    private Button _2XButton;
    [Export]
    private Button _3XButton;

    private const double OneSpeed = 1.0;
    private const double TwoSpeed = 3.0;
    private const double ThreeSpeed = 6.0;

    public override void _Ready()
    {
        Init();

        _pauseButton.Pressed += OnPauseButtonPressed;
        _1XButton.Pressed += On1XButtonPressed;
        _2XButton.Pressed += On2XButtonPressed;
        _3XButton.Pressed += On3XButtonPressed;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.Quoteleft || keyEvent.Keycode == Key.P)
                {
                    _pauseButton.ButtonPressed = true;
                    OnPauseButtonPressed();
                }
                if (keyEvent.Keycode == Key.Key1)
                {
                    _1XButton.ButtonPressed = true;
                    On1XButtonPressed();
                }
                if (keyEvent.Keycode == Key.Key2)
                {
                    _2XButton.ButtonPressed = true;
                    On2XButtonPressed();
                }
                if (keyEvent.Keycode == Key.Key3)
                {
                    _3XButton.ButtonPressed = true;
                    On3XButtonPressed();
                }
            }
        }
    }

    private void OnPauseButtonPressed()
    {
        Engine.TimeScale = 0.0;
        Settings.TheBattleSpeed = Settings.BattleSpeed.Pause;
    }

    private void On1XButtonPressed()
    {
        Engine.TimeScale = OneSpeed;
        Settings.TheBattleSpeed = Settings.BattleSpeed.One;
        XButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    private void On2XButtonPressed()
    {
        Engine.TimeScale = TwoSpeed;
        Settings.TheBattleSpeed = Settings.BattleSpeed.Two;
        XButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    private void On3XButtonPressed()
    {
        Engine.TimeScale = ThreeSpeed;
        Settings.TheBattleSpeed = Settings.BattleSpeed.Three;
        XButtonPressed?.Invoke(this, EventArgs.Empty);
    }

    private void Init()
    {
        switch (Settings.TheBattleSpeed)
        {
            case Settings.BattleSpeed.Pause:
                Engine.TimeScale = 0.0;
                _pauseButton.ButtonPressed = true;
                break;
            case Settings.BattleSpeed.One:
                Engine.TimeScale = OneSpeed;
                _1XButton.ButtonPressed = true;
                break;
            case Settings.BattleSpeed.Two:
                Engine.TimeScale = TwoSpeed;
                _2XButton.ButtonPressed = true;
                break;
            case Settings.BattleSpeed.Three:
                Engine.TimeScale = ThreeSpeed;
                _3XButton.ButtonPressed = true;
                break;
        }
    }
}
