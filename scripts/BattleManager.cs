using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

public partial class BattleManager : Node2D
{
    public event EventHandler<bool> BattleEnded;

    public const int TurnThreshold = 100;

    [Export]
    public bool DebugMovement = false;

    public List<Combatant> Combatants = []; // Initialized by Level
    public TileMapLayer TileMapLayer { get; set; } // Initialized by Level
    public Vector2I TileSize { get; set; } // Initialized by Level
    public Camera Camera { get; set; } // Initialized by Level

    private static BattleManager s_battleManager = null;

    [Export]
    private TurnOrderDisplay _turnOrderDisplay;
    [Export]
    private StatusDisplay _activeStatusDisplay;
    [Export]
    private StatusDisplay _targetedStatusDisplay;

    private Combatant _activeCombatant = null;
    private Combatant _targetedCombatant = null;

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
        int enemyCount = 0;
        foreach (Combatant combatant in Combatants)
        {
            if (combatant.MyTeam == Combatant.Team.Enemy)
            {
                enemyCount++;
            }
        }
        string enemyString = enemyCount == 1 ? "enemy" : "enemies";
        MessageLog.Get().Write(enemyCount + " " + enemyString + " encountered!", true, false);
        AdvanceTurnOrder();
        UpdateForNewCombatantTurn();
    }

    public override void _Process(double delta)
    {
        _targetedCombatant = GetTargetedCombatant();
        UpdateTargetedStatusDisplay(_targetedCombatant);
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
            MessageLog.Get().Write("Victory!", true, false);
            BattleEnded?.Invoke(this, isVictory);
        }
        else
        {
            // TODO UI elements here
            MessageLog.Get().Write("Defeat!", true, false);
            BattleEnded?.Invoke(this, isVictory);
        }
    }

    // Returns null if there is no Combatant at mouse position
    private Combatant GetTargetedCombatant()
    {
        // Update _targetedCombatant to the combatant the mouse is hovering over
        Vector2 mousePos = GetGlobalMousePosition();
        Vector2I hoveredCoords = TileMapLayer.LocalToMap(mousePos);
        Combatant result = null;
        foreach (Combatant combatant in Combatants)
        {
            Vector2I combatantCoords = TileMapLayer.LocalToMap(combatant.Position);
            if (combatantCoords == hoveredCoords)
            {
                result = combatant;
            }
        }
        return result;

    }

    // Combatant c is intentionally Optional (nullable)
    private void UpdateTargetedStatusDisplay(Combatant c)
    {
        if (c == null)
        {
            _targetedStatusDisplay.Visible = false;
        }
        else
        {
            if (c != _activeCombatant)
            {
                _targetedStatusDisplay.Update(c.Name, c.Sprite2D.Texture, c.Status.CurrentHealth.ToString(),
                    c.Stats.Health.ToString(), c.Stats.Attack.ToString(), c.Stats.Speed.ToString(),
                    c.Stats.Movement.ToString());
                _targetedStatusDisplay.Visible = true;
            }
            else
            {
                _targetedStatusDisplay.Visible = false;
            }
        }
    }

    private void UpdateForNewCombatantTurn()
    {
        _turnOrderDisplay.Update(Combatants);
        Combatant c = Combatants.First();
        _activeCombatant = c;
        Camera.Target = c;
        _activeStatusDisplay.Update(c.Name, c.Sprite2D.Texture, c.Status.CurrentHealth.ToString(),
            c.Stats.Health.ToString(), c.Stats.Attack.ToString(), c.Stats.Speed.ToString(),
            c.Stats.Movement.ToString());
        c.InitializeTurn();
    }

    private void OnCombatantTurnEnded(object sender, EventArgs e)
    {
        Combatants.Sort(new Combatant.SortDescendingAccumulatedSpeed());
        if (CheckIfACombatantHasTurn() == false)
        {
            AdvanceTurnOrder();
        }
        UpdateForNewCombatantTurn();
    }

    private void OnStatusDamageTaken(object sender, Combatant c)
    {
        _turnOrderDisplay.Update(Combatants);
        if (c == _activeCombatant)
        {
            _activeStatusDisplay.Update(c.Name, c.Sprite2D.Texture, c.Status.CurrentHealth.ToString(),
                c.Stats.Health.ToString(), c.Stats.Attack.ToString(), c.Stats.Speed.ToString(),
                c.Stats.Movement.ToString());
        }
        if (c == _targetedCombatant)
        {
            _targetedStatusDisplay.Update(c.Name, c.Sprite2D.Texture, c.Status.CurrentHealth.ToString(),
                c.Stats.Health.ToString(), c.Stats.Attack.ToString(), c.Stats.Speed.ToString(),
                c.Stats.Movement.ToString());
        }
    }

    private void OnStatusDied(object sender, Combatant combatant)
    {
        // TODO @Incomplete Lots more cleanup needed when a Status informs that a Combatant died
        Combatants.Remove(combatant);
        CheckBattleOver();
        _turnOrderDisplay.Update(Combatants);
    }
}
