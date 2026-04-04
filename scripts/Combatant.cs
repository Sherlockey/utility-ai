using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

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
    public string DisplayName = "";
    [Export]
    public Team MyTeam { get; private set; }
    [Export]
    public Brain Brain { get; private set; }
    [Export]
    public Stats Stats { get; private set; }
    [Export]
    public Status Status { get; private set; }
    [Export]
    public Movement Movement { get; private set; }
    [Export]
    public Sprite2D Sprite2D { get; private set; }

    public List<IAbility> Abilities = [];
    public TurnState CurrentTurnState = TurnState.Waiting;
    public int BattleIndex;
    public (Dictionary<Vector2I, int>, Dictionary<Vector2I, Vector2I>) DistPrevMaps = new();

    [Export]
    private Node _abilitiesParent;
    [Export]
    private Timer _turnStartTimer;
    [Export]
    private Timer _turnEndTimer;

    private const int NumDecisions = 0;

    public override void _Ready()
    {
        Status.Died += OnStatusDied;
        foreach (Node node in _abilitiesParent.GetChildren())
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
        if (BattleManager.Get().DebugManualControl == false)
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
                            ability.Execute(this, targets);
                            Status.AbilitiesRemaining -= 1;
                        }
                    }
                }
            }
        }
    }

    public async void InitializeTurn()
    {
        MessageLog.Get().Write("\n:: " + DisplayName + "'s turn start", false);

        // Set state
        Status.CurrentMovement = Stats.Movement;
        Status.AbilitiesRemaining = Stats.AbilitiesPerTurn;
        CurrentTurnState = TurnState.Active;

        //Gather walkable cells
        Vector2I currentCoords = BattleManager.Get().TileMapLayer.LocalToMap(Position);
        DistPrevMaps = Utils.WalkableCoordsDistAndPrev(currentCoords, Status.CurrentMovement, MyTeam);

        // Display walkable coords
        BattleManager battleManager = BattleManager.Get();
        foreach (KeyValuePair<Vector2I, int> kvp in DistPrevMaps.Item1)
        {
            battleManager.TileMapLayer.SetCell(kvp.Key, 4, new(0, 0), 0);
        }

        if (BattleManager.Get().DebugManualControl == true)
        {
            return;
        }

        // Delay
        _turnStartTimer.Start();
        // TODO can this be placed after Brain.MakeDecisionList(this)?
        await ToSignal(_turnStartTimer, Timer.SignalName.Timeout);

        // Ask Brain for an ability and target to apply that ability on
        List<Decision> decisionList = Brain.MakeDecisionList(this);
        Decision resultDecision = decisionList[0];

        // Message log display top NumDecisions decisions
        for (int i = 0; i < NumDecisions; i++)
        {
            if (i < decisionList.Count)
            {
                // TODO add if statements for whether there is actually a movement or
                // actually an ability used?
                // TODO cut this down or handle it somewhere else. Maybe an event?
                Decision decision = decisionList[i];
                MessageLog.Get().Write("Decision " + (i + 1) + " with utility: " + decision.TotalUtility.ToString("F2") + "\n- Move from: " + currentCoords + " to: " + decision.MoveLocation + ".\n- Use ability: " + decision.Ability.GetDisplayName() + ". \n- With target(s): " + TargetListToString(decision.Targets));
            }
        }

        // Do movement over time
        Vector2I resultCoords = resultDecision.MoveLocation;
        if (resultCoords != currentCoords)
        {
            await Movement.WalkTo(resultCoords, DistPrevMaps.Item2);
        }

        // Use ability
        if (resultDecision.Targets.Count > 0 && resultDecision.AbilityUtility > 0.0f)
        {
            resultDecision.Ability.Execute(this, resultDecision.Targets);
        }

        // Delay
        _turnEndTimer.Start();
        await ToSignal(_turnEndTimer, Timer.SignalName.Timeout);

        EndTurn();
    }

    public void EndTurn()
    {
        BattleManager battleManager = BattleManager.Get();
        // Reset walkable background cells to regular background cells
        foreach (KeyValuePair<Vector2I, int> kvp in DistPrevMaps.Item1)
        {
            battleManager.TileMapLayer.SetCell(kvp.Key, 0, new(0, 0), 0);
        }
        // Reset state and notify that combatant's turn has ended
        if (CurrentTurnState == TurnState.Active)
        {
            Status.AccumulatedSpeed %= BattleManager.TurnThreshold;
            CurrentTurnState = TurnState.Waiting;
            TurnEnded?.Invoke(this, EventArgs.Empty);
        }
    }

    private static string TargetListToString(List<Combatant> targets)
    {
        if (targets.Count == 0)
        {
            return "none";
        }

        string result = "";
        for (int i = 0; i < targets.Count; i++)
        {
            result += targets[i].DisplayName;
            if (i + 1 < targets.Count)
            {
                result += ", ";
            }
        }
        return result;
    }

    private void OnStatusDied(object sender, Combatant combatant)
    {
        if (CurrentTurnState == TurnState.Active)
        {
            EndTurn();
        }
    }

    // Sorts by descending order of accumlated speed.
    // Ties go to whichever combatant was added to the array first (stored in BattleIndex).
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

    // Sorts by descending order of # of iterations taken until TurnThreshold is met.
    // Ties go to whichever combatant was added to the array first (stored in BattleIndex).
    public class SortIterationsUntilTurn : IComparer<Combatant>
    {
        public int Compare(Combatant x, Combatant y)
        {
            int xIterations = IterationsUntilTurn(x);
            int yIterations = IterationsUntilTurn(y);
            int result = xIterations.CompareTo(yIterations);
            if (result == 0)
            {
                result = x.BattleIndex.CompareTo(y.BattleIndex);
            }
            return result;
        }
    }

    private static int IterationsUntilTurn(Combatant combatant)
    {
        int iterations = 0;
        int tempSpeed = combatant.Status.AccumulatedSpeed;
        while (tempSpeed < BattleManager.TurnThreshold)
        {
            tempSpeed += combatant.Stats.Speed;
            iterations++;
        }
        return iterations;
    }
}
