using Godot;

using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Level : Node2D
{
    public int Difficulty = 0;
    public Dictionary<string, Dictionary<Type, MovementUtilityFunction>> MovementDict = [];
    public Dictionary<string, Dictionary<Type, AbilityUtilityFunction>> AbilityDict = [];

    [Export]
    private EnemyPositioning _enemyPositioning = EnemyPositioning.Random;
    [Export]
    private EnemyComposition _enemyComposition = EnemyComposition.Random;
    [Export]
    private TileMapLayer _tileMapLayer;
    [Export]
    private DebugTileMapLayer _debugTileMapLayer;
    [Export]
    private Camera _camera;
    [Export]
    private Marker2D[] _allyCombatantSpawnMarkers;
    [Export]
    private Marker2D[] _enemyCombatantSpawnMarkers; // Do not use, only for initializing List
    [Export]
    private PackedScene _battleManagerScene;
    [Export]
    private PackedScene[] _allyCombatantScenes;
    [Export]
    private SceneArray[] _enemyCombatantScenesSets;
    [Export]
    private string[] _nextLevelPaths;

    private const float EnemyScalePercent = 0.10f;
    private const string LevelOneScenePath = "res://scenes/level_one.tscn";
    private const float EndOfGameDelay = 1.5f;

    private readonly Random _random = new();

    private List<Marker2D> _enemySpawnMarkersList = [];

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
        _enemySpawnMarkersList = [.. _enemyCombatantSpawnMarkers];
        InstantiateChildren();
    }

    public void InstantiateChildren()
    {
        BattleManager battleManager = _battleManagerScene.Instantiate<BattleManager>();
        // Instantiate combatants and register them in battleManager
        AddAllyCombatants(battleManager);
        AddEnemyCombatants(battleManager);
        battleManager.TileMapLayer = _tileMapLayer;
        battleManager.DebugTileMapLayer = _debugTileMapLayer;
        battleManager.TileSize = _tileMapLayer.TileSet.TileSize;
        battleManager.Camera = _camera;
        if (Difficulty + 1 > 1)
        {
            battleManager.LevelInfoPopup.DifficultyLabel.Text = "Level " + (Difficulty + 1)
                + "\nEnemies grow stronger...";
        }
        else
        {
            battleManager.LevelInfoPopup.DifficultyLabel.Text = "Level " + (Difficulty + 1);
        }
        battleManager.BattleEnded += OnBattleManagerBattleEnded;
        // Only add to scene tree once necessary children are already in scene tree
        AddChild(battleManager);
    }

    private void AddAllyCombatants(BattleManager battleManager)
    {
        for (int i = 0; i < _allyCombatantScenes.Length; i++)
        {
            Combatant combatant = _allyCombatantScenes[i].Instantiate<Combatant>();
            AddChild(combatant);
            UpdateUtility(combatant); // order matters here; must be after combatant in scene tree
            combatant.Position = _allyCombatantSpawnMarkers[i].Position;
            battleManager.Combatants.Add(combatant);
        }
    }

    private void AddEnemyCombatants(BattleManager battleManager)
    {
        if (_enemyCombatantScenesSets.Length == 0)
        {
            return;
        }

        int sceneIndex = _random.Next(0, _enemyCombatantScenesSets.Length);
        PackedScene[] scenesSet = _enemyCombatantScenesSets[sceneIndex].Scenes;
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

            // Handle scaling combatant based upon Level Difficulty
            float scaling = Difficulty * EnemyScalePercent;
            combatant.Stats.ApplyScaling(scaling); // Ordering matters here  w/ AddChild

            AddChild(combatant);

            // Handle positioning combatant
            int markerIndex = i % _enemySpawnMarkersList.Count;
            if (_enemyPositioning == EnemyPositioning.Random)
            {
                markerIndex = _random.Next(0, _enemySpawnMarkersList.Count);
                combatant.Position = _enemySpawnMarkersList[markerIndex].Position;
            }
            else if (_enemyPositioning == EnemyPositioning.Linear)
            {
                combatant.Position = _enemySpawnMarkersList[markerIndex].Position;
            }
            _enemySpawnMarkersList.RemoveAt(markerIndex);

            battleManager.Combatants.Add(combatant);
        }
    }

    private void UpdateUtility(Combatant combatant)
    {
        // Movement
        if (MovementDict.ContainsKey(combatant.DisplayName))
        {
            for (int i = 0; i < combatant.Brain.MovementUtilities.Count; i++)
            {
                MovementUtilityFunction movementUtility = combatant.Brain.MovementUtilities[i];
                if (MovementDict[combatant.DisplayName].TryGetValue(movementUtility.GetType(),
                    out MovementUtilityFunction savedUtility))
                {
                    combatant.Brain.MovementUtilities[i] = savedUtility;
                }
            }
        }

        // Ability
        if (AbilityDict.ContainsKey(combatant.DisplayName))
        {
            for (int i = 0; i < combatant.Brain.AbilityUtilities.Count; i++)
            {
                AbilityUtilityFunction abilityUtility = combatant.Brain.AbilityUtilities[i];
                if (AbilityDict[combatant.DisplayName].TryGetValue(abilityUtility.GetType(),
                    out AbilityUtilityFunction savedUtility))
                {
                    combatant.Brain.AbilityUtilities[i] = savedUtility;
                }
            }
        }
    }

    private async void OnBattleManagerBattleEnded(object sender, BattleEndInfo battleEndInfo)
    {
        if (battleEndInfo.IsVictory)
        {
            await ToSignal(GetTree().CreateTimer(EndOfGameDelay), Timer.SignalName.Timeout);
            Debug.Assert(_nextLevelPaths.Length > 0);
            int index = 0;
            for (int i = 0; i < _nextLevelPaths.Length; i++)
            {
                if (_nextLevelPaths[i] == GetTree().CurrentScene.SceneFilePath)
                {
                    index = (i + 1) % _nextLevelPaths.Length;
                }
            }
            string path = _nextLevelPaths[index];
            if (path != "")
            {
                PackedScene nextLevelScene = GD.Load<PackedScene>(path);
                Level nextLevel = nextLevelScene.Instantiate<Level>();
                nextLevel.Difficulty = Difficulty + 1;
                nextLevel.MovementDict = battleEndInfo.MovementDict;
                nextLevel.AbilityDict = battleEndInfo.AbilityDict;
                SceneTree sceneTree = GetTree();
                sceneTree.Root.AddChild(nextLevel);
                sceneTree.CurrentScene = nextLevel;
                Free();
            }
        }
        else
        {
            await ToSignal(GetTree().CreateTimer(EndOfGameDelay), Timer.SignalName.Timeout);
            PackedScene levelOneScene = GD.Load<PackedScene>(LevelOneScenePath);
            Level levelOne = levelOneScene.Instantiate<Level>();
            levelOne.MovementDict = battleEndInfo.MovementDict;
            levelOne.AbilityDict = battleEndInfo.AbilityDict;
            SceneTree sceneTree = GetTree();
            sceneTree.Root.AddChild(levelOne);
            sceneTree.CurrentScene = levelOne;
            Free();
        }
    }
}
