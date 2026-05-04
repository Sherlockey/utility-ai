using Godot;

using System;

public partial class WeightSetUtilityControl : HBoxContainer
{
    [Export]
    public Label DisplayNameLabel { get; private set; }
    [Export]
    public Slider Slider { get; private set; }
}
