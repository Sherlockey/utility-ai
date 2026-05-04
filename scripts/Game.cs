using Godot;

using System;
using System.Collections.Generic;

public partial class Game : Node
{
    public static Game Instance { get; private set; } = null;

    [Export]
    public PackedScene OverworldScene;

    public List<Combatant> Party { get; private set; } = [];

    [Export]
    private SceneArray _defaultParty;

    private Node _currentScene;

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

    public override void _EnterTree()
    {
        // TODO: if SaveGame has a party, use that, otherwise use default
        foreach (PackedScene combatantScene in _defaultParty.Scenes)
        {
            Combatant combatant = combatantScene.Instantiate<Combatant>();
            Party.Add(combatant);
            AddChild(combatant);
            combatant.Visible = false;
        }
    }

    public override void _Ready()
    {
        ChangeSceneToPacked(OverworldScene);
    }

    public void ChangeSceneToPacked(PackedScene scene)
    {
        Node node = scene.Instantiate();
        AddChild(node);
        _currentScene?.QueueFree();
        _currentScene = node;
    }

    public void ChangeSceneToNode(Node node)
    {
        AddChild(node);
        _currentScene?.QueueFree();
        _currentScene = node;
    }
}
