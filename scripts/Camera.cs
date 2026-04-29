using Godot;

using System;

// Camera Position at the start of a turn is set by the BattleManager in UpdateForNewCombatantTurn()
public partial class Camera : Camera2D
{
    [Export]
    private float _moveRate = 16.0f;

    public Combatant Target = null; // Set by BattleManager at start of turn or if a combatant moves

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && !keyEvent.IsReleased())
        {
            if (keyEvent.Keycode == Key.Left)
            {
                Position += Vector2.Left * _moveRate;
            }
            if (keyEvent.Keycode == Key.Right)
            {
                Position += Vector2.Right * _moveRate;
            }
            if (keyEvent.Keycode == Key.Up)
            {
                Position += Vector2.Up * _moveRate;
            }
            if (keyEvent.Keycode == Key.Down)
            {
                Position += Vector2.Down * _moveRate;
            }

            if (keyEvent.Keycode == Key.Enter)
            {
                if (Target != null)
                {
                    Position = Target.Position;
                }
            }
        }
    }

    public void OnCombatantMoved(object sender, Combatant combatant)
    {
        Position = combatant.Position;
        Target = combatant;
    }
}
