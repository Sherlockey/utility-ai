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
    public Stats Stats { get; private set; }
    [Export]
    public Status Status { get; private set; }
    [Export]
    public Movement Movement;
    [Export]
    public Sprite2D Sprite2D { get; private set; }
    [Export]
    public Node AbilitiesParent;

    public List<IAbility> Abilities = [];
    public TurnState CurrentTurnState { get; set; } = TurnState.Waiting;
    public int BattleIndex { get; set; }

    public override void _Ready()
    {
        Status.Died += OnStatusDied;
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
        // Toggle whether input comes from the player or from the AI

        if (CurrentTurnState == TurnState.Waiting)
        {
            return;
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // Turn Management
            if (keyEvent.Keycode == Key.Space)
            {
                // Added to prevent ending turn of the combatant who next has a turn immediately
                // TODO is there another way to handle this?
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
                // TODO this is temporary for testing
                Vector2 mousePos = GetGlobalMousePosition();
                Vector2I selectedCoords = BattleManager.Get().TileMapLayer.LocalToMap(mousePos);
                Vector2I myCoords = BattleManager.Get().TileMapLayer.LocalToMap(Position);
                foreach (IAbility ability in Abilities)
                {
                    if (ability.IsInRange(myCoords, selectedCoords))
                    {
                        List<Combatant> targets = ability.CombatantsInAreaOfEffect(selectedCoords);
                        targets = ability.ValidatedTargets(this, targets);
                        if (targets.Count > 0)
                        {
                            ability.Apply(this, targets);
                            Status.AbilitiesRemaining -= 1;
                        }
                    }
                }
            }
            if (keyEvent.Keycode == Key.M)
            {
                Vector2I source = BattleManager.Get().TileMapLayer.LocalToMap(Position);
                List<Vector2I> graph = Utils.CoordsInDist(source, Status.CurrentMovement);
                graph = Utils.TraversableCoords(graph);
                (Dictionary<Vector2I, int>, Dictionary<Vector2I, Vector2I>) distPrevTuple =
                    Utils.Dijkstra(graph, source);
                foreach (KeyValuePair<Vector2I, int> kvp in distPrevTuple.Item1)
                {
                    // zzz working on getting rid of cells that don't have a connected path
                    if (kvp.Value <= Status.CurrentMovement
                            && distPrevTuple.Item2[kvp.Key].X < int.MaxValue
                            && distPrevTuple.Item2[kvp.Key].Y < int.MaxValue)
                    {
                        BattleManager.Get().TileMapLayer.SetCell(kvp.Key, 0, new(0, 0), 4);
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

    private void OnStatusDied(object sender, Combatant combatant)
    {
        if (CurrentTurnState == TurnState.Active)
        {
            EndTurn();
        }
    }
}
