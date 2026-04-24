using Godot;

using System;

public partial class PartyDisplay : Control
{
    public event EventHandler Closed;

    [Export]
    private Button _closeButton;

    public override void _Ready()
    {
        _closeButton.Pressed += OnCloseButtonPressed;
    }

    private void OnCloseButtonPressed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }
}
