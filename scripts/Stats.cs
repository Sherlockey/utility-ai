using Godot;

using System;

public partial class Stats : Node
{
    [Export]
    public int Health { get; set; } = 44;
    [Export]
    public int Attack { get; set; } = 4;
    [Export]
    public int Speed { get; set; } = 6;
    [Export]
    public int Movement { get; set; } = 4;
    [Export]
    public int Evasion { get; set; } = 5;
}
