using Godot;

using System;

public partial class EmbarkDisplay : PanelContainer
{
    public event EventHandler Closed;

    [Export]
    private PackedScene _selectedLevel; // TODO add level selecting?
    [Export]
    private Button _noButton;
    [Export]
    private Button _yesButton;

    public override void _Ready()
    {
        _yesButton.Pressed += OnYesButtonPressed;
        _noButton.Pressed += OnNoButtonPressed;
    }

    private void OnYesButtonPressed()
    {
        Game.Instance.ChangeSceneToLevel(_selectedLevel);
    }

    private void OnNoButtonPressed()
    {
        Visible = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }
}
