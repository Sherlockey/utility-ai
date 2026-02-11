using Godot;

using System;
using System.Linq;

public partial class BattleManager : Node
{
	public const int TurnThreshold = 100;

	[Export]
	private Combatant[] _combatants;

	public override void _Ready()
	{
		// Subscribe to turn ended for each combatant and set their battle indices
		for (int i = 0; i < _combatants.Length; i++)
		{
			Combatant combatant = _combatants[i];
			if (combatant != null)
			{
				combatant.TurnEnded += OnCombatantTurnEnded;
				combatant.BattleIndex = i;
			}
		}

		AdvanceTurnOrder();
		Combatant firstCombatant = _combatants.First();
		MovementComponent movementComponent = firstCombatant.MovementComponent;
		movementComponent.CurrentMovement = movementComponent.Movement;
		firstCombatant.CurrentTurnState = Combatant.TurnState.Active;
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

	public void InitializeCombatantsTileSize(Vector2I tileSize)
	{
		foreach (Combatant combatant in _combatants)
		{
			combatant.MovementComponent.TileSize = tileSize;
		}
	}

	private bool CheckIfACombatantHasTurn()
	{
		return _combatants.First().AccumulatedSpeed >= TurnThreshold;
	}

	private void AdvanceTurnOrder()
	{
		bool anyCombatantHasTurn = false;
		while (!anyCombatantHasTurn)
		{
			foreach (Combatant combatant in _combatants)
			{
				combatant.AccumulatedSpeed += combatant.Speed;
				if (combatant.AccumulatedSpeed >= TurnThreshold)
				{
					anyCombatantHasTurn = true;
				}
			}
		}
		Array.Sort(_combatants, new Combatant.SortDescendingAccumulatedSpeed());
	}

	private void DisplayTurnOrder()
	{
		foreach (Combatant combatant in _combatants)
		{
			GD.Print(combatant.Name + ": " + combatant.AccumulatedSpeed);
		}
	}

	private void OnCombatantTurnEnded(object sender, EventArgs e)
	{
		Array.Sort(_combatants, new Combatant.SortDescendingAccumulatedSpeed());
		if (CheckIfACombatantHasTurn() == false)
		{
			AdvanceTurnOrder();
		}
		Combatant combatant = _combatants.First();
		MovementComponent movementComponent = combatant.MovementComponent;
		movementComponent.CurrentMovement = movementComponent.Movement;
		combatant.CurrentTurnState = Combatant.TurnState.Active;
	}
}
