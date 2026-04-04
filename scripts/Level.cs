using Godot;

using System;
using System.Collections.Generic;

public partial class Level : Node2D
{
    [Export]
    private EnemyComposition _enemyComposition = EnemyComposition.RoundRobin;
    [Export]
    private TileMapLayer _tileMapLayer;
    [Export]
    private DebugTileMapLayer _debugTileMapLayer;
    [Export]
    private Camera _camera;
    [Export]
    private Marker2D[] _allyCombatantSpawnMarkers;
    [Export]
    private Marker2D[] _enemyCombatantSpawnMarkers;
    [Export]
    private PackedScene _battleManagerScene;
    [Export]
    private PackedScene[] _allyCombatantScenes;
    [Export]
    private PackedScene[] _enemyCombatantScenes;

    private Random _random = new();

    private enum EnemyComposition
    {
        RoundRobin, RandomSet, Random,
    }

    public override void _Ready()
    {
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
        battleManager.BattleEnded += OnBattleManagerBattleEnded;
        // Only add to scene tree once necessary children are already in scene tree
        AddChild(battleManager);
    }

    private void AddAllyCombatants(BattleManager battleManager)
    {
        for (int i = 0; i < _allyCombatantSpawnMarkers.Length; i++)
        {
            if (_allyCombatantScenes.Length > 0)
            {
                int index = i % _allyCombatantScenes.Length;
                Combatant combatant = _allyCombatantScenes[index].Instantiate<Combatant>();
                AddChild(combatant);
                combatant.Position = _allyCombatantSpawnMarkers[i].Position;
                battleManager.Combatants.Add(combatant);
            }
        }
    }

    private void AddEnemyCombatants(BattleManager battleManager)
    {
        if (_enemyComposition == EnemyComposition.RoundRobin)
        {
            for (int i = 0; i < _enemyCombatantSpawnMarkers.Length; i++)
            {
                if (_enemyCombatantScenes.Length > 0)
                {
                    int index = i % _enemyCombatantScenes.Length;
                    Combatant combatant = _enemyCombatantScenes[index].Instantiate<Combatant>();
                    AddChild(combatant);
                    combatant.Position = _enemyCombatantSpawnMarkers[i].Position;
                    battleManager.Combatants.Add(combatant);
                }
            }
        }
        else if (_enemyComposition == EnemyComposition.RandomSet)
        {

        }
        else if (_enemyComposition == EnemyComposition.Random)
        {
            for (int i = 0; i < _enemyCombatantSpawnMarkers.Length; i++)
            {
                if (_enemyCombatantScenes.Length > 0)
                {
                    int index = _random.Next(0, _enemyCombatantScenes.Length);
                    Combatant combatant = _enemyCombatantScenes[index].Instantiate<Combatant>();
                    AddChild(combatant);
                    combatant.Position = _enemyCombatantSpawnMarkers[i].Position;
                    battleManager.Combatants.Add(combatant);
                }
            }
        }
    }

    private async void OnBattleManagerBattleEnded(object sender, bool isVictory)
    {
        if (isVictory)
        {
            // TODO goto next level
            await ToSignal(GetTree().CreateTimer(1.5f), Timer.SignalName.Timeout);
            GetTree().ReloadCurrentScene();
        }
        else
        {
            // TODO restart game
            await ToSignal(GetTree().CreateTimer(1.5f), Timer.SignalName.Timeout);
            GetTree().ReloadCurrentScene();
        }
    }
}
