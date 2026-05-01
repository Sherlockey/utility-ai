using Godot;

using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Level : Node2D
{
    public int Attempt = 1;

    [Export]
    private string _displayName = "Missing Level Name";
    [Export]
    private int _enemyLevel = 1;
    [Export]
    private EnemyPositioning _enemyPositioning = EnemyPositioning.Random;
    [Export]
    private EnemyComposition _enemyComposition = EnemyComposition.Random;

    [Export]
    private SceneArray[] _enemyCombatantScenesSets;
    [Export]
    private Node _enemySpawnParent;
    [Export]
    private Node _allySpawnParent;
    [Export]
    private Node _partyParent;
    [Export]
    private PackedScene _battleManagerScene;
    [Export]
    private TileMapLayer _tileMapLayer;
    [Export]
    private DebugTileMapLayer _debugTileMapLayer;
    [Export]
    private Camera _camera;

    private readonly Random _random = new();

    private enum EnemyComposition
    {
        Random, Linear,
    }

    private enum EnemyPositioning
    {
        Random, Linear,
    }

    public override void _Ready()
    {
        InstantiateChildren();
    }

    public void InstantiateChildren()
    {
        BattleManager battleManager = _battleManagerScene.Instantiate<BattleManager>();
        AddAllyCombatants(battleManager);
        AddEnemyCombatants(battleManager);
        battleManager.TileMapLayer = _tileMapLayer;
        battleManager.DebugTileMapLayer = _debugTileMapLayer;
        battleManager.TileSize = _tileMapLayer.TileSet.TileSize;
        battleManager.Camera = _camera;
        battleManager.LevelName = _displayName;
        battleManager.Attempt = Attempt;
        battleManager.EnemyLevel = _enemyLevel;
        battleManager.BattleEnded += OnBattleManagerBattleEnded;
        // Only add to scene tree once necessary children are already in scene tree
        AddChild(battleManager);
    }

    private void AddAllyCombatants(BattleManager battleManager)
    {
        List<Combatant> party = Game.Instance.Party;
        List<Marker2D> allySpawnMarkers = [];
        foreach (Node child in _allySpawnParent.GetChildren())
        {
            if (child is Marker2D marker2D)
            {
                allySpawnMarkers.Add(marker2D);
            }
        }
        Debug.Assert(allySpawnMarkers.Count >= party.Count);

        for (int i = 0; i < party.Count; i++)
        {
            Combatant combatant = party[i];
            if (combatant.Status.CurrentHealth > 0)
            {
                _partyParent.AddChild(combatant);
                combatant.Position = allySpawnMarkers[i].Position;
                combatant.Visible = true;
                combatant.ProcessMode = ProcessModeEnum.Inherit;
                battleManager.Combatants.Add(combatant);
            }
        }
    }

    private void AddEnemyCombatants(BattleManager battleManager)
    {
        if (_enemyCombatantScenesSets.Length == 0)
        {
            return;
        }

        // Choose random combatant scenes set, collect enemySpawnMarkers
        int sceneIndex = _random.Next(0, _enemyCombatantScenesSets.Length);
        PackedScene[] scenesSet = _enemyCombatantScenesSets[sceneIndex].Scenes;
        List<Marker2D> enemySpawnMarkers = [];
        foreach (Node child in _enemySpawnParent.GetChildren())
        {
            if (child is Marker2D marker2D)
            {
                enemySpawnMarkers.Add(marker2D);
            }
        }
        Debug.Assert(enemySpawnMarkers.Count >= scenesSet.Length);

        // Calculate average level of player party for potential experience reward penalty
        int partyLevel = 0;
        foreach (Combatant combatant in Game.Instance.Party)
        {
            partyLevel += combatant.Stats.Level;
        }
        if (Game.Instance.Party.Count != 0)
        {
            partyLevel /= Game.Instance.Party.Count;
        }

        for (int i = 0; i < scenesSet.Length; i++)
        {
            // Handle choosing combatant from scenesSet
            int combatantIndex = i;
            if (_enemyComposition == EnemyComposition.Random)
            {
                combatantIndex = _random.Next(0, scenesSet.Length);
            }
            else if (_enemyComposition == EnemyComposition.Linear)
            {
                combatantIndex = i;
            }
            Combatant combatant = scenesSet[combatantIndex].Instantiate<Combatant>();
            combatant.Stats.BaseLevel = _enemyLevel;
            AddChild(combatant);
            battleManager.Combatants.Add(combatant);

            // Handle positioning combatant
            int markerIndex = i % enemySpawnMarkers.Count;
            if (_enemyPositioning == EnemyPositioning.Random)
            {
                markerIndex = _random.Next(0, enemySpawnMarkers.Count);
                combatant.Position = enemySpawnMarkers[markerIndex].Position;
            }
            else if (_enemyPositioning == EnemyPositioning.Linear)
            {
                combatant.Position = enemySpawnMarkers[markerIndex].Position;
            }
            enemySpawnMarkers.RemoveAt(markerIndex);

            // Calculate experience and knowledge rewards
            int enemyLevelDeficit = 0;
            if (combatant.Stats.Level < partyLevel)
            {
                enemyLevelDeficit = partyLevel - combatant.Stats.Level;
            }

            float scalar = 1.0f;
            float overleveledPenalty = -0.1f;
            scalar += enemyLevelDeficit * overleveledPenalty;
            if (scalar < 0.1f)
            {
                scalar = 0.1f;
            }

            int experienceReward = (int)(combatant.Stats.ExperienceReward * scalar);
            int knowledgeReward = (int)(combatant.Stats.KnowledgeReward * scalar);

            battleManager.ExperienceReward += experienceReward;
            battleManager.KnowledgeReward += knowledgeReward;
        }
    }

    private void OnBattleManagerBattleEnded(object sender, BattleManager.BattleEndType battleEndType)
    {
        if (battleEndType == BattleManager.BattleEndType.Victory)
        {
            foreach (Node child in _partyParent.GetChildren())
            {
                if (child is Combatant combatant)
                {
                    _partyParent.RemoveChild(combatant);
                }
            }

            string path = SceneFilePath;
            PackedScene levelScene = GD.Load<PackedScene>(path);
            Level nextLevel = levelScene.Instantiate<Level>();
            nextLevel.Attempt = Attempt + 1;
            Game.Instance.ChangeSceneToNode(nextLevel);
        }
        else if (battleEndType == BattleManager.BattleEndType.Loss
            || battleEndType == BattleManager.BattleEndType.RanAway)
        {
            ReturnToOverworld();
        }
    }

    private void ReturnToOverworld()
    {
        foreach (Node child in _partyParent.GetChildren())
        {
            if (child is Combatant combatant)
            {
                _partyParent.RemoveChild(combatant);
            }
        }
        foreach (Combatant combatant in Game.Instance.Party)
        {
            combatant.Status.ResetForNewBattle();
        }
        Game.Instance.ChangeSceneToPacked(Game.Instance.OverworldScene);
    }
}
