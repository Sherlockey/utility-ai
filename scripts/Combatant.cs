using Godot;

using System;
using System.Collections.Generic;

public partial class Combatant : Node2D
{
	public enum TurnState
	{
		Active,
		Waiting,
	}

	public event EventHandler TurnEnded;

	[Export]
	public Node AbilitiesNode;
	[Export]
	public Stats Stats { get; private set; }
	[Export]
	public Status Status { get; private set; }
	[Export]
	public Movement Movement;

	public List<IAbility> Abilities = [];
	public TurnState CurrentTurnState { get; set; } = TurnState.Waiting;
	public int AccumulatedSpeed { get; set; } = 0;
	public int BattleIndex { get; set; }

	public override void _Ready()
	{
		foreach (Node node in AbilitiesNode.GetChildren())
		{
			IAbility ability = node as IAbility;
			if (ability != null)
			{
				Abilities.Add(ability);
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (CurrentTurnState == TurnState.Waiting)
		{
			return;
		}

		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			// Turn Management
			if (keyEvent.Keycode == Key.Space)
			{
				// TODO: is there another way to handle this?
				GetViewport().SetInputAsHandled();
				EndTurn();
			}

			if (keyEvent.Keycode == Key.T)
			{
				// TODO: this is temporary for testing
				Attack attack = Abilities[0] as Attack;
				if (attack != null)
				{
					attack._accuracy = 100;
					attack._damage = Stats.Attack * Stats.Attack;
					attack.Apply();
				}
			}
		}
	}

	public void EndTurn()
	{
		if (CurrentTurnState == TurnState.Active)
		{
			AccumulatedSpeed %= BattleManager.TurnThreshold;
			CurrentTurnState = TurnState.Waiting;
			TurnEnded?.Invoke(this, EventArgs.Empty);
		}
	}

	// Sorts by descending order of accumlated speed. Ties go to whichever combatant was 
	// added to the array first.
	public class SortDescendingAccumulatedSpeed : IComparer<Combatant>
	{
		public int Compare(Combatant x, Combatant y)
		{
			int result = y.AccumulatedSpeed.CompareTo(x.AccumulatedSpeed);
			if (result == 0)
			{
				result = x.BattleIndex.CompareTo(y.BattleIndex);
			}
			return result;
		}
	}
}
