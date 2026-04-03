using Godot;

using System;

public partial class Stats : Node
{
    [Export]
    public int Health = 44;
    [Export]
    public int Attack = 4;
    [Export]
    public int Speed = 6;
    [Export]
    public int Movement = 4;
    [Export]
    public int Accuracy = 0;
    [Export]
    public int Evasion = 10;
    [Export]
    public int AbilitiesPerTurn = 1;
}
