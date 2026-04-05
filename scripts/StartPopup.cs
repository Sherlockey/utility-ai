using Godot;

using System;

public partial class StartPopup : PanelContainer
{
    public TimeDisplay TimeDisplay = null; // set by battleManager on Ready

    public void OnTimeDisplayXButtonPressed(object sender, EventArgs e)
    {
        Visible = false;
        QueueFree();
    }

    public override void _ExitTree()
    {
        if (TimeDisplay != null)
        {
            TimeDisplay.XButtonPressed -= OnTimeDisplayXButtonPressed;
        }
    }
}
