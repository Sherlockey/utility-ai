using Godot;

using System;
using System.Collections.Generic;

public partial class Game : Node
{
    public static Game Instance { get; private set; } = null;

    [Export]
    private SceneArray _defaultParty;
    [Export]
    private PackedScene _overworldScene;

    private Node _currentScene;
    public List<Combatant> Party { get; private set; } = [];

    public Game()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Free();
        }
    }

    public override void _Ready()
    {
        // TODO: if SaveGame has a party, use that, otherwise use default
        foreach (PackedScene combatantScene in _defaultParty.Scenes)
        {
            Combatant combatant = combatantScene.Instantiate<Combatant>();
            Party.Add(combatant);
        }

        Overworld overworld = _overworldScene.Instantiate<Overworld>();
        AddChild(overworld);
        _currentScene = overworld;
    }

    public void ChangeSceneToLevel(PackedScene levelScene)
    {
        Level level = levelScene.Instantiate<Level>();
        AddChild(level);
        _currentScene.QueueFree();
        _currentScene = level;
    }
}
