using Godot;

using System;
using System.Collections.Generic;

public partial class Level : Node2D
{
    [Export]
    private TileMapLayer _tileMapLayer;
    [Export]
    private DebugTileMapLayer _debugTileMapLayer;
    [Export]
    private Camera _camera;
    [Export]
    private PackedScene[] _combatantScenes;
    [Export]
    private PackedScene _battleManagerScene;

    private readonly List<Marker2D> _combatantSpawnMarkers = [];

    public override void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            if (child is Marker2D marker2D)
            {
                _combatantSpawnMarkers.Add(marker2D);
            }
        }
        InstantiateChildren();
    }

    public void InstantiateChildren()
    {
        BattleManager battleManager = _battleManagerScene.Instantiate<BattleManager>();
        // Instantiate combatants and register them in battleManager
        for (int i = 0; i < _combatantScenes.Length; i++)
        {
            Combatant combatant = _combatantScenes[i].Instantiate<Combatant>();
            AddChild(combatant);
            combatant.Position = _combatantSpawnMarkers[i].Position;
            battleManager.Combatants.Add(combatant);
        }
        battleManager.TileMapLayer = _tileMapLayer;
        battleManager.DebugTileMapLayer = _debugTileMapLayer;
        battleManager.TileSize = _tileMapLayer.TileSet.TileSize;
        battleManager.Camera = _camera;
        battleManager.BattleEnded += OnBattleManagerBattleEnded;
        // Only add to scene tree once necessary children are already in scene tree
        AddChild(battleManager);
    }

    private void OnBattleManagerBattleEnded(object sender, bool isVictory)
    {
        if (isVictory)
        {
            // TODO goto next level
            GetTree().ReloadCurrentScene();
        }
        else
        {
            // TODO restart game
            GetTree().ReloadCurrentScene();
        }
    }
}
