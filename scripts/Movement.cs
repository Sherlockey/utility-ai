using Godot;

using System;

public partial class Movement : Node
{
    [Export]
    private Combatant _combatant;

    public override void _Input(InputEvent @event)
    {
        if (_combatant.CurrentTurnState == Combatant.TurnState.Waiting)
        {
            return;
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // Movement
            if (keyEvent.Keycode == Key.A)
            {
                TryMove(new Vector2I(-BattleManager.Get().TileSize.X, 0));
            }
            if (keyEvent.Keycode == Key.D)
            {
                TryMove(new Vector2I(BattleManager.Get().TileSize.X, 0));
            }
            if (keyEvent.Keycode == Key.S)
            {
                TryMove(new Vector2I(0, BattleManager.Get().TileSize.Y));
            }
            if (keyEvent.Keycode == Key.W)
            {
                TryMove(new Vector2I(0, -BattleManager.Get().TileSize.Y));
            }
        }
    }

    private bool TryMove(Vector2I movement)
    {
        if (_combatant.Status.CurrentMovement > 0)
        {
            _combatant.GlobalPosition = _combatant.GlobalPosition + movement;
            _combatant.Status.CurrentMovement -= 1;
            return true;
        }

        return false;
    }
}
