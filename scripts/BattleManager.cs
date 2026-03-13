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
    public Camera Camera { get; set; } // Initialized by Level

    private static BattleManager s_battleManager = null;

    [Export]
    private VBoxContainer _turnOrderEntryVBox;
    [Export]
    private PackedScene _turnOrderEntryScene;

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
        UpdateTurnOrderDisplay();
        Combatant firstCombatant = Combatants.First();
        Camera.Target = firstCombatant;
        firstCombatant.InitializeTurn();
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

    private void UpdateTurnOrderDisplay()
    {
        foreach (Node child in _turnOrderEntryVBox.GetChildren())
        {
            _turnOrderEntryVBox.RemoveChild(child);
            child.QueueFree();
        }
        int i = 1;
        foreach (Combatant combatant in Combatants)
        {
            TurnOrderEntry turnOrderEntry = _turnOrderEntryScene.Instantiate<TurnOrderEntry>();
            turnOrderEntry.Label.Text = i.ToString();
            turnOrderEntry.TextureRect.Texture = combatant.Sprite2D.Texture;
            _turnOrderEntryVBox.AddChild(turnOrderEntry);
            i++;
        }
    }

    private void OnCombatantTurnEnded(object sender, EventArgs e)
    {
        Combatants.Sort(new Combatant.SortDescendingAccumulatedSpeed());
        if (CheckIfACombatantHasTurn() == false)
        {
            AdvanceTurnOrder();
        }
        UpdateTurnOrderDisplay();
        Combatant firstCombatant = Combatants.First();
        Camera.Target = firstCombatant;
        firstCombatant.InitializeTurn();
    }

    private void OnStatusDied(object sender, Combatant combatant)
    {
        GD.Print("BattleManager.OnStatusDied not fully implemented yet!");
        // TODO Lots more cleanup needed when a Status informs that a Combatant died
        Combatants.Remove(combatant);
        UpdateTurnOrderDisplay();
    }
}
