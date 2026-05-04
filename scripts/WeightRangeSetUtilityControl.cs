using Godot;

using System;

public partial class WeightRangeSetUtilityControl : HBoxContainer
{
    [Export]
    public Label DisplayNameLabel { get; private set; }
    [Export]
    public Slider Slider { get; private set; }
    [Export]
    public Label RangeLabel { get; private set; }
    [Export]
    public Button LeftButton { get; private set; }
    [Export]
    public Button RightButton { get; private set; }

    public override void _Ready()
    {
        LeftButton.Pressed += OnLeftButtonPressed;
        RightButton.Pressed += OnRightButtonPressed;
    }

    private void OnLeftButtonPressed()
    {
        int range = RangeLabel.Text.ToInt();
        if (range > 0)
        {
            range--;
            RangeLabel.Text = range.ToString();
        }
    }

    private void OnRightButtonPressed()
    {
        int range = RangeLabel.Text.ToInt();
        if (range < int.MaxValue)
        {
            range++;
            RangeLabel.Text = range.ToString();
        }
    }
}
