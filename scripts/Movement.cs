using Godot;

using System;

public partial class Movement : Node
{
	[Export]
	private Node2D _node2D;
	[Export]
	private Combatant _combatant;

	public override void _Input(InputEvent @event)
	{
		if (_combatant.GetTurnState() == Combatant.TurnState.Waiting)
		{
			return;
		}

		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			// Movement
			if (keyEvent.Keycode == Key.A)
			{
				TryMoveLeft();
			}
			if (keyEvent.Keycode == Key.S)
			{
				TryMoveDown();
			}
			if (keyEvent.Keycode == Key.D)
			{
				TryMoveRight();
			}
			if (keyEvent.Keycode == Key.W)
			{
				TryMoveUp();
			}

			// Turn Management
			if (keyEvent.Keycode == Key.Space)
			{
				// TODO: change this
				GetViewport().SetInputAsHandled();
				_combatant.EndTurn();
			}
		}
	}

	private bool TryMoveLeft()
	{
		if (_combatant.GetCurrentMovement() > 0)
		{
			int x = -16;
			int y = 0;
			Vector2 newPosition = new Vector2(_node2D.GlobalPosition.X + x, _node2D.GlobalPosition.Y + y);
			_node2D.GlobalPosition = newPosition;

			_combatant.SetCurrentMovement(_combatant.GetCurrentMovement() - 1);
			return true;
		}

		return false;
	}

	private bool TryMoveRight()
	{
		if (_combatant.GetCurrentMovement() > 0)
		{
			int x = 16;
			int y = 0;
			Vector2 newPosition = new Vector2(_node2D.GlobalPosition.X + x, _node2D.GlobalPosition.Y + y);
			_node2D.GlobalPosition = newPosition;

			_combatant.SetCurrentMovement(_combatant.GetCurrentMovement() - 1);
			return true;
		}

		return false;
	}

	private bool TryMoveDown()
	{
		if (_combatant.GetCurrentMovement() > 0)
		{
			int x = 0;
			int y = 24;
			Vector2 newPosition = new Vector2(_node2D.GlobalPosition.X + x, _node2D.GlobalPosition.Y + y);
			_node2D.GlobalPosition = newPosition;

			_combatant.SetCurrentMovement(_combatant.GetCurrentMovement() - 1);
			return true;
		}

		return false;
	}

	private bool TryMoveUp()
	{
		if (_combatant.GetCurrentMovement() > 0)
		{
			int x = 0;
			int y = -24;
			Vector2 newPosition = new Vector2(_node2D.GlobalPosition.X + x, _node2D.GlobalPosition.Y + y);
			_node2D.GlobalPosition = newPosition;

			_combatant.SetCurrentMovement(_combatant.GetCurrentMovement() - 1);
			return true;
		}

		return false;
	}
}
