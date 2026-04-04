using Godot;

using System;

[GlobalClass]
public partial class SceneArray : Resource
{
    [Export]
    public PackedScene[] Scenes = [];
}
