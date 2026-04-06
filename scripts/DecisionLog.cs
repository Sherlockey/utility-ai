using Godot;

using System;

public partial class DecisionLog : Log
{
    private static DecisionLog s_decisionLog = null;

    public DecisionLog()
    {
        s_decisionLog = this;
    }

    public static DecisionLog Get()
    {
        return s_decisionLog;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.F2)
            {
                Visible = !Visible;
            }
        }
    }
}
