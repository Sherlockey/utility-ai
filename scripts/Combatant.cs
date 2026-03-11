using Godot;

using System;
using System.Collections.Generic;

public partial class Combatant : Node2D
{
    public enum Team
    {
        Enemy,
        Ally,
    }
    public enum TurnState
    {
        Active,
        Waiting,
    }

    public event EventHandler TurnEnded;

    [Export]
    public Team MyTeam { get; private set; }
    [Export]
    public Node AbilitiesParent;
    [Export]
    public Stats Stats { get; private set; }
    [Export]
    public Status Status { get; private set; }
    [Export]
    public Movement Movement;

    public List<IAbility> Abilities = [];
    public TurnState CurrentTurnState { get; set; } = TurnState.Waiting;
    public int BattleIndex { get; set; }

    public override void _Ready()
    {
        foreach (Node node in AbilitiesParent.GetChildren())
        {
            if (node is IAbility ability)
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
                // Don't do anything if combatant doesn't have any AbilitiesRemaining
                if (Status.AbilitiesRemaining <= 0)
                {
                    return;
                }
                // TODO: this is temporary for testing
                Vector2 mousePos = GetGlobalMousePosition();
                Vector2I tileCoords = BattleManager.Get().TileMapLayer.LocalToMap(mousePos);
                Combatant target = null;
                foreach (Combatant combatant in BattleManager.Get().Combatants)
                {
                    Vector2I combatantCoords = BattleManager.Get().TileMapLayer.LocalToMap
                        (combatant.GlobalPosition);
                    if (combatantCoords == tileCoords)
                    {
                        target = combatant;
                        break;
                    }
                }
                if (target != null)
                {
                    foreach (IAbility ability in Abilities)
                    {
                        Vector2I myCoords = BattleManager.Get().TileMapLayer.LocalToMap(Position);
                        if (ability.IsInRange(myCoords, tileCoords) && ability.IsTargetValid(this, target))
                        {
                            Combatant[] targets = { target };
                            ability.Apply(this, targets);
                            GD.Print("Stats.Attack for " + Name + " is " + Stats.Attack);
                            Status.AbilitiesRemaining -= 1;
                            return; // TODO this is temporary
                        }
                    }
                }
            }
        }
    }

    public void InitializeTurn()
    {
        Status.CurrentMovement = Stats.Movement;
        Status.AbilitiesRemaining = Stats.AbilitiesPerTurn;
        CurrentTurnState = TurnState.Active;
    }

    public void EndTurn()
    {
        if (CurrentTurnState == TurnState.Active)
        {
            Status.AccumulatedSpeed %= BattleManager.TurnThreshold;
            CurrentTurnState = TurnState.Waiting;
            TurnEnded?.Invoke(this, EventArgs.Empty);
        }
    }

    // Sorts by descending order of accumlated speed. Ties go to whichever combatant was
    // added to the array first (stored in BattleIndex).
    public class SortDescendingAccumulatedSpeed : IComparer<Combatant>
    {
        public int Compare(Combatant x, Combatant y)
        {
            int result = y.Status.AccumulatedSpeed.CompareTo(x.Status.AccumulatedSpeed);
            if (result == 0)
            {
                result = x.BattleIndex.CompareTo(y.BattleIndex);
            }
            return result;
        }
    }
}
