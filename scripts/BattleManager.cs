using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

public partial class BattleManager : Node2D
{
    public event EventHandler<bool> BattleEnded;
    public event EventHandler<TeamControl> TeamControlChanged;

    public const int TurnThreshold = 100;

    [Export]
    public TeamControl MyTeamControl = TeamControl.None;
    [Export]
    public AudioStreamPlayer AbilitySFXPlayer { get; private set; }
    [Export]
    public LevelInfoPopup LevelInfoPopup { get; private set; }

    public List<Combatant> Combatants = []; // Initialized by Level
    public TileMapLayer TileMapLayer; // Initialized by Level
    public DebugTileMapLayer DebugTileMapLayer; // Initialized by Level
    public Vector2I TileSize; // Initialized by Level
    public Camera Camera; // Initialized by Level
    public int ExperienceReward; // Initialized by Level
    public int KnowledgeReward; // Initialized by Level
    public string LevelName; // Initialized by Level
    public int Attempt; // Initialized by Level
    public int EnemyLevel; // Initialized by Level

    // Item1 = enemy influence, Item2 = ally influence
    public Dictionary<Vector2I, (int, int)> InfluenceMap { get; private set; } = [];
    public Dictionary<Vector2I, float> ScoreMap { get; private set; } = [];

    private static BattleManager s_battleManager = null;

    [Export]
    private TurnOrderDisplay _turnOrderDisplay;
    [Export]
    private StatusDisplay _activeStatusDisplay;
    [Export]
    private StatusDisplay _targetedStatusDisplay;
    [Export]
    private UtilityDisplay _utilityDisplay;
    [Export]
    private TimeDisplay _timeDisplay;
    [Export]
    private StartPopup _startPopup;

    private const float EndOfGameDelay = 1.5f;

    private bool _isBattleOver = false;
    private Combatant _activeCombatant = null;
    private Combatant _targetedCombatant = null;
    private float _attemptBonusScalar = 0.0f;

    public enum TeamControl
    {
        None, Ally, Enemy, All,
    }

    public BattleManager()
    {
        s_battleManager = this;
    }

    public static BattleManager Get()
    {
        return s_battleManager;
    }

    public override void _Ready()
    {
        if (Settings.TheBattleSpeed != Settings.BattleSpeed.Pause)
        {
            _startPopup.Free();
        }
        else
        {
            _timeDisplay.XButtonPressed += _startPopup.OnTimeDisplayXButtonPressed;
            _startPopup.TimeDisplay = _timeDisplay;
        }

        // Attempt bonus calculation
        string attemptBonusText = "";
        _attemptBonusScalar = 0.1f * (Attempt - 1);
        if (Attempt > 1)
        {
            attemptBonusText = "\n(Reward Bonus: " + (_attemptBonusScalar * 100).ToString("F0") + "%)";
        }

        // Set LevelInfoPopup text
        LevelInfoPopup.LevelAttemptsLabel.Text =
            LevelName + '\n' + "Attempt: " + Attempt + attemptBonusText;

        DebugTileMapLayer.Initialize(TileMapLayer);
        InitializeInfluenceMap();

        // Subscribe to events from combatants, set their battle indices, count enemies
        int enemyCount = 0;
        for (int i = 0; i < Combatants.Count; i++)
        {
            Combatant combatant = Combatants[i];
            combatant.BattleIndex = i;

            combatant.TurnEnded += OnCombatantTurnEnded;
            combatant.Status.DamageTaken += OnStatusDamageTaken;
            combatant.Status.Died += OnStatusDied;
            combatant.Movement.Moved += Camera.OnCombatantMoved;
            TeamControlChanged += combatant.OnBattleManagerTeamControlChanged;

            if (combatant.MyTeam == Combatant.Team.Enemy)
            {
                enemyCount++;
            }
            else
            {
                _utilityDisplay.Combatants.Add(combatant);
            }
        }

        _utilityDisplay.RefreshDisplay();

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

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.F5)
            {
                RefreshDebugDisplays();
            }
            if (keyEvent.Keycode == Key.Z)
            {
                if (MyTeamControl != TeamControl.None)
                {
                    MyTeamControl = TeamControl.None;
                    MessageLog.Get().Write("Team control changed to: " + MyTeamControl.ToString());
                    TeamControlChanged?.Invoke(this, MyTeamControl);
                }
            }
            if (keyEvent.Keycode == Key.X)
            {
                if (MyTeamControl != TeamControl.Ally)
                {
                    MyTeamControl = TeamControl.Ally;
                    MessageLog.Get().Write("Team control changed to: " + MyTeamControl.ToString());
                    TeamControlChanged?.Invoke(this, MyTeamControl);
                }
            }
            if (keyEvent.Keycode == Key.C)
            {
                if (MyTeamControl != TeamControl.Enemy)
                {
                    MyTeamControl = TeamControl.Enemy;
                    MessageLog.Get().Write("Team control changed to: " + MyTeamControl.ToString());
                    TeamControlChanged?.Invoke(this, MyTeamControl);
                }
            }
            if (keyEvent.Keycode == Key.V)
            {
                if (MyTeamControl != TeamControl.All)
                {
                    MyTeamControl = TeamControl.All;
                    MessageLog.Get().Write("Team control changed to: " + MyTeamControl.ToString());
                    TeamControlChanged?.Invoke(this, MyTeamControl);
                }
            }
        }
    }

    public void DrawInfluenceMap()
    {
        int max = 1;
        foreach (Vector2I coords in InfluenceMap.Keys)
        {
            int item1 = InfluenceMap[coords].Item1;
            int item2 = InfluenceMap[coords].Item2;
            int maxItem = Mathf.Max(item1, item2);
            max = Mathf.Max(maxItem, max);
        }
        float scalar = 255.0f / max;

        foreach (Vector2I coords in InfluenceMap.Keys)
        {
            int red = Mathf.RoundToInt(InfluenceMap[coords].Item1 * scalar);
            int blue = Mathf.RoundToInt(InfluenceMap[coords].Item2 * scalar);
            DebugTileMapLayer.SetCellModulate
                (coords, Color.Color8((byte)red, 0, (byte)blue));
        }
    }

    private bool CheckIfACombatantHasTurn()
    {
        return Combatants.First().Status.AccumulatedSpeed >= TurnThreshold;
    }

    // TODO this doesn't work perfectly. Does not allow for duplicates. If one combatant were to "lap"
    // another, it will not show them twice. Really there should be a "forecast" of the next "9" turns
    // displayed. This would also mean I wouldn't have to be sorting the actual list of
    // alive Combatants which would be another improvement.
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
        Combatants.Sort(new Combatant.SortIterationsUntilTurn());
    }

    private void CheckBattleOver()
    {
        int enemyCount = 0;
        int allyCount = 0;
        foreach (Combatant combatant in Combatants)
        {
            if (combatant.MyTeam == Combatant.Team.Enemy)
            {
                enemyCount++;
            }
            else if (combatant.MyTeam == Combatant.Team.Ally)
            {
                allyCount++;
            }
        }
        if (enemyCount == 0)
        {
            BattleEnd(true);
        }
        else if (allyCount == 0)
        {
            BattleEnd(false);
        }
    }

    private async void BattleEnd(bool isVictory)
    {
        _isBattleOver = true;

        // Eventually add animation for showing experience points etc being added?
        await ToSignal(GetTree().CreateTimer(EndOfGameDelay), Timer.SignalName.Timeout);

        foreach (Combatant combatant in Game.Instance.Party)
        {
            combatant.TurnEnded -= OnCombatantTurnEnded;
            combatant.Status.DamageTaken -= OnStatusDamageTaken;
            combatant.Status.Died -= OnStatusDied;
            combatant.Movement.Moved -= Camera.OnCombatantMoved;
        }

        if (isVictory)
        {
            MessageLog.Get().Write("Victory!", true, false);
            HandleRewards();
        }
        else
        {
            MessageLog.Get().Write("Defeat!", true, false);
        }
        BattleEnded?.Invoke(this, isVictory);
    }

    private void HandleRewards()
    {
        foreach (Combatant combatant in Game.Instance.Party)
        {
            combatant.Stats.GainExperiencePoints((int)(ExperienceReward * (_attemptBonusScalar + 1)));
            combatant.Stats.GainKnowledgePoints((int)(KnowledgeReward * (_attemptBonusScalar + 1)));
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
                _targetedStatusDisplay.Update(c.DisplayName, c.Sprite2D.Texture, c.Stats.Level.ToString(), c.Stats.ExperiencePoints.ToString(), c.Status.CurrentHealth.ToString(), c.Stats.Health.ToString(),
                    c.Stats.Attack.ToString(), c.Stats.Speed.ToString(), c.Stats.Movement.ToString(),
                    c.Stats.Accuracy.ToString(), c.Stats.Evasion.ToString());
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
        UpdateInfluenceMap();
        if (DebugTileMapLayer.Visible)
        {
            DrawInfluenceMap();
        }
        _turnOrderDisplay.Update(Combatants);
        Combatant c = Combatants.First();
        _activeCombatant = c;
        Camera.Position = c.Position;
        Camera.Target = c;
        _activeStatusDisplay.Update(c.DisplayName, c.Sprite2D.Texture, c.Stats.Level.ToString(), c.Stats.ExperiencePoints.ToString(), c.Status.CurrentHealth.ToString(), c.Stats.Health.ToString(),
            c.Stats.Attack.ToString(), c.Stats.Speed.ToString(), c.Stats.Movement.ToString(),
            c.Stats.Accuracy.ToString(), c.Stats.Evasion.ToString());
        c.InitializeTurn();
    }

    private void InitializeInfluenceMap()
    {
        foreach (Vector2I coords in TileMapLayer.GetUsedCellsById(0)) // 0 = background, aka traversable
        {
            InfluenceMap[coords] = (0, 0);
        }
    }

    private void UpdateInfluenceMap()
    {
        foreach (Vector2I coords in InfluenceMap.Keys)
        {
            InfluenceMap[coords] = (0, 0);
        }

        List<Dictionary<Vector2I, int>> enemyInfluenceMaps = [];
        List<Dictionary<Vector2I, int>> allyInfluenceMaps = [];
        foreach (Combatant combatant in Combatants)
        {
            if (combatant.MyTeam == Combatant.Team.Enemy)
            {
                Dictionary<Vector2I, int> combatantInfluenceMap =
                    Utils.MakeCombatantInfluenceMapNaive(combatant);
                enemyInfluenceMaps.Add(combatantInfluenceMap);
            }
            else if (combatant.MyTeam == Combatant.Team.Ally)
            {
                Dictionary<Vector2I, int> combatantInfluenceMap =
                    Utils.MakeCombatantInfluenceMapNaive(combatant);
                allyInfluenceMaps.Add(combatantInfluenceMap);
            }
        }
        foreach (Vector2I coords in InfluenceMap.Keys)
        {
            foreach (Dictionary<Vector2I, int> entry in enemyInfluenceMaps)
            {
                InfluenceMap[coords] =
                    (
                        InfluenceMap[coords].Item1 + entry.GetValueOrDefault(coords, 0),
                        InfluenceMap[coords].Item2
                    );
            }
            foreach (Dictionary<Vector2I, int> entry in allyInfluenceMaps)
            {
                InfluenceMap[coords] =
                    (
                        InfluenceMap[coords].Item1,
                        InfluenceMap[coords].Item2 + entry.GetValueOrDefault(coords, 0)
                    );
            }
        }
    }

    private void RefreshDebugDisplays()
    {
        if (DebugTileMapLayer.Visible)
        {
            UpdateInfluenceMap(); // has some gameplay implications, but is fine due to current ordering
            DrawInfluenceMap();
        }
    }

    private void OnCombatantTurnEnded(object sender, EventArgs e)
    {
        if (!_isBattleOver)
        {
            Combatants.Sort(new Combatant.SortIterationsUntilTurn());
            if (CheckIfACombatantHasTurn() == false)
            {
                AdvanceTurnOrder();
            }
            UpdateForNewCombatantTurn();
        }
    }

    private void OnStatusDamageTaken(object sender, Combatant c)
    {
        _turnOrderDisplay.Update(Combatants);
        if (c == _activeCombatant)
        {
            _activeStatusDisplay.Update(c.DisplayName, c.Sprite2D.Texture, c.Stats.Level.ToString(), c.Stats.ExperiencePoints.ToString(), c.Status.CurrentHealth.ToString(), c.Stats.Health.ToString(),
                c.Stats.Attack.ToString(), c.Stats.Speed.ToString(), c.Stats.Movement.ToString(),
                c.Stats.Accuracy.ToString(), c.Stats.Evasion.ToString());
        }
        if (c == _targetedCombatant)
        {
            _targetedStatusDisplay.Update(c.DisplayName, c.Sprite2D.Texture, c.Stats.Level.ToString(), c.Stats.ExperiencePoints.ToString(), c.Status.CurrentHealth.ToString(), c.Stats.Health.ToString(),
                c.Stats.Attack.ToString(), c.Stats.Speed.ToString(), c.Stats.Movement.ToString(),
                c.Stats.Accuracy.ToString(), c.Stats.Evasion.ToString());
        }
    }

    private void OnStatusDied(object sender, Combatant combatant)
    {
        // TODO @Incomplete Lots more cleanup needed when a Status informs that a Combatant died
        Combatants.Remove(combatant);
        CheckBattleOver();
        _turnOrderDisplay.Update(Combatants);
        _utilityDisplay.Combatants.Remove(combatant);
    }
}
