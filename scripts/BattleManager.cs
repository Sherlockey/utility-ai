using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

public partial class BattleManager : Node
{
    public event EventHandler<bool> BattleEnded;

    public const int TurnThreshold = 100;

    public List<Combatant> Combatants = []; // Initialized by Level
    public TileMapLayer TileMapLayer { get; set; } // Initialized by Level
    public Vector2I TileSize { get; set; } // Initialized by Level
    public Camera Camera { get; set; } // Initialized by Level

    private static BattleManager s_battleManager = null;

    [Export]
    private VBoxContainer _turnOrderEntryVBox;
    [Export]
    private StatusDisplay _activeStatusDisplay;
    [Export]
    private StatusDisplay _targetedStatusDisplay;
    [Export]
    private PackedScene _turnOrderEntryScene;

    public BattleManager()
    {
        s_battleManager = this;
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
                combatant.Status.DamageTaken += OnStatusDamageTaken;
                combatant.Status.Died += OnStatusDied;
                combatant.BattleIndex = i;
            }
        }
        AdvanceTurnOrder();
        UpdateTurnOrderDisplay();
        Combatant c = Combatants.First();
        _activeStatusDisplay.Update(c.Name, c.Sprite2D.Texture, c.Status.CurrentHealth.ToString(),
            c.Stats.Health.ToString(), c.Stats.Attack.ToString(), c.Stats.Speed.ToString(),
            c.Stats.Movement.ToString());
        Camera.Target = c;
        c.InitializeTurn();
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
        // Remove old entires from TurnOrderVBox
        foreach (Node child in _turnOrderEntryVBox.GetChildren())
        {
            _turnOrderEntryVBox.RemoveChild(child);
            child.QueueFree();
        }
        // Repopulate TurnOrderVBox
        int i = 1;
        foreach (Combatant combatant in Combatants)
        {
            // TODO decouple the health bar from the turn order
            TurnOrderEntry turnOrderEntry = _turnOrderEntryScene.Instantiate<TurnOrderEntry>();
            turnOrderEntry.Label.Text = i.ToString();
            turnOrderEntry.TextureRect.Texture = combatant.Sprite2D.Texture;
            turnOrderEntry.HealthBar.TextureProgressBar.MaxValue = combatant.Stats.Health;
            turnOrderEntry.HealthBar.TextureProgressBar.Value = combatant.Status.CurrentHealth;
            _turnOrderEntryVBox.AddChild(turnOrderEntry);
            i++;
            if (i > 9) // TODO 9 is currently a hardcoded # of entries allowed in turn order display
            {
                break;
            }
        }
    }

    private void CheckBattleOver()
    {
        List<Combatant> allyCombatants = [];
        List<Combatant> enemyCombatants = [];
        foreach (Combatant combatant in Combatants)
        {
            if (combatant.MyTeam == Combatant.Team.Ally)
            {
                allyCombatants.Add(combatant);
            }
            else if (combatant.MyTeam == Combatant.Team.Enemy)
            {
                enemyCombatants.Add(combatant);
            }
        }
        if (allyCombatants.Count == 0)
        {
            BattleEnd(false);
        }
        else if (enemyCombatants.Count == 0)
        {
            BattleEnd(true);
        }
    }

    private void BattleEnd(bool isVictory)
    {
        if (isVictory)
        {
            // TODO UI elements here
            GD.Print("Victory!");
            BattleEnded?.Invoke(this, isVictory);
        }
        else
        {
            // TODO UI elements here
            GD.Print("Defeat!");
            BattleEnded?.Invoke(this, isVictory);
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
        Combatant c = Combatants.First();
        _activeStatusDisplay.Update(c.Name, c.Sprite2D.Texture, c.Status.CurrentHealth.ToString(),
            c.Stats.Health.ToString(), c.Stats.Attack.ToString(), c.Stats.Speed.ToString(),
            c.Stats.Movement.ToString());
        Camera.Target = c;
        c.InitializeTurn();
    }

    private void OnStatusDamageTaken(object sender, EventArgs e)
    {
        UpdateTurnOrderDisplay();
    }

    private void OnStatusDied(object sender, Combatant combatant)
    {
        GD.Print("BattleManager.OnStatusDied not fully implemented yet!");
        // TODO Lots more cleanup needed when a Status informs that a Combatant died
        Combatants.Remove(combatant);
        CheckBattleOver();
        UpdateTurnOrderDisplay();
    }
}
