using Godot;

using System.Collections.Generic;

public abstract partial class MovementUtilityFunction : Node
{
    [Export]
    public string DisplayName { get; private set; } = "Missing Display Name";
    [Export(PropertyHint.Range, "0.0, 1.0")]
    public float Weight = 0.5f;
    [Export]
    public int Cost { get; private set; } = 100;

    [Export]
    protected PackedScene _setUtilityControlScene;

    public abstract float CalculateUtility(
        Vector2I evaluatedCoords,
        Dictionary<Vector2I, (int, int)> influenceMap,
        Combatant.Team sourceTeam);

    public virtual Control InstantiateSetUtilityControl()
    {
        WeightSetUtilityControl setUtilityControl = _setUtilityControlScene.Instantiate<WeightSetUtilityControl>();
        setUtilityControl.DisplayNameLabel.Text = DisplayName;
        setUtilityControl.Slider.Value = Weight;
        setUtilityControl.Slider.ValueChanged += OnSetUtilityControlSliderValueChanged;

        return setUtilityControl;
    }

    protected void OnSetUtilityControlSliderValueChanged(double value)
    {
        Weight = (float)value;
    }
}
