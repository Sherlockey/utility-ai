using Godot;

using System;

public partial class Camera : Camera2D
{
    // public Node2D Target { get; set; }

    // public override void _Process(double delta)
    // {
    //     if (Target != null)
    //     {
    //         Position = Target.Position;
    //     }
    // }

    public void OnCombatantMoved(object sender, Combatant combatant)
    {
        Position = combatant.Position;
    }
}
