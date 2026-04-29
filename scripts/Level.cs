using Godot;

using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Level : Node2D
{
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

    private const float EnemyScalePercent = 0.20f;

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
        if (_enemyLevel + 1 > 1)
        {
            battleManager.LevelInfoPopup.DifficultyLabel.Text = "Level " + (_enemyLevel + 1)
                + "\nEnemies grow stronger...";
        }
        else
        {
            battleManager.LevelInfoPopup.DifficultyLabel.Text = "Level " + (_enemyLevel + 1);
        }
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

            // Handle scaling enemy combatant
            float scaling = (_enemyLevel - 1) * EnemyScalePercent;
            combatant.Stats.ApplyScaling(scaling); // Ordering matters here  w/ AddChild

            AddChild(combatant);

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

            battleManager.Combatants.Add(combatant);
        }
    }

    private void OnBattleManagerBattleEnded(object sender, bool isVictory)
    {
        if (isVictory)
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
            Game.Instance.ChangeScene(levelScene);
        }
        else
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
        Game.Instance.ChangeScene(Game.Instance.OverworldScene);
    }
}
