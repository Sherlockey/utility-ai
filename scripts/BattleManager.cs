using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

public partial class BattleManager : Node
{
	public const int TURN_THRESHOLD = 100;

	[Export]
	private Combatant[] _combatants;

	public override void _Ready()
	{
		foreach (Combatant combatant in _combatants)
		{
			combatant.TurnEnded += OnCombatantTurnEnded;
		}

		AdvanceTurnOrder();
		Combatant firstCombatant = _combatants.First();
		firstCombatant.SetCurrentMovement(firstCombatant.GetMovement());
		firstCombatant.SetTurnState(Combatant.TurnState.Active);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == Key.I)
			{
				DisplayTurnOrder();
			}
		}
	}

	private bool CheckIfACombatantHasTurn()
	{
		return _combatants.First().GetAccumulatedSpeed() >= TURN_THRESHOLD;
	}

	private void AdvanceTurnOrder()
	{
		bool anyCombatantHasTurn = false;
		while (!anyCombatantHasTurn)
		{
			foreach (Combatant combatant in _combatants)
			{
				combatant.IncrementAccumulatedSpeed();
				if (combatant.GetAccumulatedSpeed() >= TURN_THRESHOLD)
				{
					anyCombatantHasTurn = true;
				}
			}
		}
		Array.Sort(_combatants, new SortByDescendingAccumulatedSpeed());
	}

	private void DisplayTurnOrder()
	{
		foreach (Combatant combatant in _combatants)
		{
			GD.Print(combatant.Name + ": " + combatant.GetAccumulatedSpeed());
		}
	}

	private void OnCombatantTurnEnded(object sender, EventArgs e)
	{
		Array.Sort(_combatants, new SortByDescendingAccumulatedSpeed());
		if (CheckIfACombatantHasTurn() == false)
		{
			AdvanceTurnOrder();
		}
		Combatant combatant = _combatants.First();
		combatant.SetCurrentMovement(combatant.GetMovement());
		combatant.SetTurnState(Combatant.TurnState.Active);
	}

	// Sorts by descending order of accumlated speed. Ties go to whichever combatant was 
	// added to the array first.
	public class SortByDescendingAccumulatedSpeed : IComparer<Combatant>
	{
		public int Compare(Combatant x, Combatant y)
		{
			return y.GetAccumulatedSpeed().CompareTo(x.GetAccumulatedSpeed());
		}
	}
}
