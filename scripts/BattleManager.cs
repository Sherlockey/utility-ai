using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

public partial class BattleManager : Node
{
    public const int TurnThreshold = 100;
    public List<Combatant> Combatants = []; // Initialized by Level
    public TileMapLayer TileMapLayer { get; set; } // Initialized by Level
    public Vector2I TileSize { get; set; } // Initialized by Level

    private static BattleManager s_battleManager = null;

    public BattleManager()
    {
        if (s_battleManager == null)
        {
            s_battleManager = this;
        }
        else
        {
            QueueFree();
        }
    }

    public override void _Ready()
    {
        // Subscribe to turn ended for each combatant and set their battle indices
        for (int i = 0; i < Combatants.Count; i++)
        {
            Combatant combatant = Combatants[i];
            if (combatant != null)
            {
                combatant.TurnEnded += OnCombatantTurnEnded;
                combatant.Status.Died += OnStatusDied;
                combatant.BattleIndex = i;
            }
        }
        AdvanceTurnOrder();
        Combatant firstCombatant = Combatants.First();
        firstCombatant.InitializeTurn();
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

    public static BattleManager Get()
    {
        return s_battleManager;
    }

    private bool CheckIfACombatantHasTurn()
    {
        return Combatants.First().Status.AccumulatedSpeed >= TurnThreshold;
    }

    private void AdvanceTurnOrder()
    {
        bool anyCombatantHasTurn = false;
        while (!anyCombatantHasTurn)
        {
            foreach (Combatant combatant in Combatants)
            {
                combatant.Status.AccumulatedSpeed += combatant.Stats.Speed;
                if (combatant.Status.AccumulatedSpeed >= TurnThreshold)
                {
                    anyCombatantHasTurn = true;
                }
            }
        }
        Combatants.Sort(new Combatant.SortDescendingAccumulatedSpeed());
    }

    private void DisplayTurnOrder()
    {
        foreach (Combatant combatant in Combatants)
        {
            GD.Print(combatant.Name + ": " + combatant.Status.AccumulatedSpeed);
        }
    }

    private void OnCombatantTurnEnded(object sender, EventArgs e)
    {
        Combatants.Sort(new Combatant.SortDescendingAccumulatedSpeed());
        if (CheckIfACombatantHasTurn() == false)
        {
            AdvanceTurnOrder();
        }
        Combatant firstCombatant = Combatants.First();
        firstCombatant.InitializeTurn();
    }

    private void OnStatusDied(object sender, Combatant combatant)
    {
        GD.Print("BattleManager.OnStatusDied not implemented yet!");
        // TODO: Lots more cleanup needed when a Status informs that a Combatant died
        Combatants.Remove(combatant);
    }
}
