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
    public Node AbilitiesParent;
    [Export]
    public Stats Stats { get; private set; }
    [Export]
    public Status Status { get; private set; }
    [Export]
    public Movement Movement;
    [Export]
    public Team MyTeam { get; private set; }

    public List<IAbility> Abilities = [];
    public TurnState CurrentTurnState { get; set; } = TurnState.Waiting;
    public int AccumulatedSpeed { get; set; } = 0;
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
                // TODO: this is temporary for testing
                Vector2 mousePos = GetGlobalMousePosition();
                Vector2I tileCoordinates = BattleManager.Get().TileMapLayer.LocalToMap(mousePos);
                Combatant target = null;
                foreach (Combatant combatant in BattleManager.Get().Combatants)
                {
                    Vector2I combatantCoordinates = BattleManager.Get().TileMapLayer.LocalToMap
                        (combatant.GlobalPosition);
                    if (combatantCoordinates == tileCoordinates)
                    {
                        target = combatant;
                        break;
                    }
                }
                if (target != null)
                {
                    if (Abilities[0] is Attack attack)
                    {
                        attack._accuracy = 100;
                        GD.Print("Stats.Attack for " + Name + " is " + Stats.Attack);
                        attack._damage = Stats.Attack * Stats.Attack;
                        Combatant[] targets = { target };
                        attack.Apply(targets);
                        // reduce number of Abilities in Status by one
                        // should check number of Abilities in Status before allowing to do an Ability
                        // End turn if abilities is <=0, otherwise allow to choose again?
                    }
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
    // added to the array first (stored in BattleIndex).
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
