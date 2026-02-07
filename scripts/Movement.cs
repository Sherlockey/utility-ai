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
			if (keyEvent.Keycode == Key.W)
			{
				int x = 0;
				int y = -24;
				Vector2 newPosition = new Vector2(_node2D.GlobalPosition.X + x, _node2D.GlobalPosition.Y + y);
				_node2D.GlobalPosition = newPosition;
			}
			if (keyEvent.Keycode == Key.A)
			{
				int x = -16;
				int y = 0;
				Vector2 newPosition = new Vector2(_node2D.GlobalPosition.X + x, _node2D.GlobalPosition.Y + y);
				_node2D.GlobalPosition = newPosition;
			}
			if (keyEvent.Keycode == Key.S)
			{
				int x = 0;
				int y = 24;
				Vector2 newPosition = new Vector2(_node2D.GlobalPosition.X + x, _node2D.GlobalPosition.Y + y);
				_node2D.GlobalPosition = newPosition;
			}
			if (keyEvent.Keycode == Key.D)
			{
				int x = 16;
				int y = 0;
				Vector2 newPosition = new Vector2(_node2D.GlobalPosition.X + x, _node2D.GlobalPosition.Y + y);
				_node2D.GlobalPosition = newPosition;
			}
			if (keyEvent.Keycode == Key.Q)
			{
				int x = -16;
				int y = -24;
				Vector2 newPosition = new Vector2(_node2D.GlobalPosition.X + x, _node2D.GlobalPosition.Y + y);
				_node2D.GlobalPosition = newPosition;
			}
			if (keyEvent.Keycode == Key.E)
			{
				int x = 16;
				int y = -24;
				Vector2 newPosition = new Vector2(_node2D.GlobalPosition.X + x, _node2D.GlobalPosition.Y + y);
				_node2D.GlobalPosition = newPosition;
			}
			if (keyEvent.Keycode == Key.Z)
			{
				int x = -16;
				int y = 24;
				Vector2 newPosition = new Vector2(_node2D.GlobalPosition.X + x, _node2D.GlobalPosition.Y + y);
				_node2D.GlobalPosition = newPosition;
			}
			if (keyEvent.Keycode == Key.C)
			{
				int x = 16;
				int y = 24;
				Vector2 newPosition = new Vector2(_node2D.GlobalPosition.X + x, _node2D.GlobalPosition.Y + y);
				_node2D.GlobalPosition = newPosition;
			}

			if (keyEvent.Keycode == Key.Space)
			{
				// TODO: change this
				GetViewport().SetInputAsHandled();
				_combatant.EndTurn();
			}
		}
	}
}
