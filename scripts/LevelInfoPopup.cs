using Godot;

using System;

public partial class LevelInfoPopup : PanelContainer
{
    [Export]
    public Label LevelAttemptsLabel;
    [Export]
    private double _displayDurationMsec = 3000.0;

    private ulong _startTime;

    public override void _Ready()
    {
        _startTime = Time.GetTicksMsec();
    }

    public override void _Process(double delta)
    {
        if (Time.GetTicksMsec() > _startTime + _displayDurationMsec)
        {
            QueueFree();
        }
    }
}
