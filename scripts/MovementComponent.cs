using Godot;

using System;

public partial class MovementComponent : Node
{
	public Vector2I TileSize { get; set; } = new Vector2I(16, 24);

	[Export]
	private Combatant _combatant;
	[Export]
	public int Movement { get; private set; } = 4;

	public int CurrentMovement { get; set; }

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
				TryMove(new Vector2I(-TileSize.X, 0));
			}
			if (keyEvent.Keycode == Key.D)
			{
				TryMove(new Vector2I(TileSize.X, 0));
			}
			if (keyEvent.Keycode == Key.S)
			{
				TryMove(new Vector2I(0, TileSize.Y));
			}
			if (keyEvent.Keycode == Key.W)
			{
				TryMove(new Vector2I(0, -TileSize.Y));
			}
		}
	}

	private bool TryMove(Vector2I movement)
	{
		if (CurrentMovement > 0)
		{
			_combatant.GlobalPosition = _combatant.GlobalPosition + movement;
			CurrentMovement -= 1;
			return true;
		}

		return false;
	}
}
