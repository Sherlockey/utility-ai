using Godot;

using System.Collections.Generic;
using System.Diagnostics;

public abstract partial class AbilityUtilityFunction : Node
{
    [Export(PropertyHint.Range, "0.0, 1.0")]
    public float Weight = 0.5f;

    public abstract int CalculateValue(IAbility ability, Combatant user, List<Combatant> targets);

    [Export]
    protected string _displayName = "Missing Display Name";
    [Export]
    protected PackedScene _setUtilityControlScene;

    public virtual float CalculateUtility(int min, int max, int value)
    {
        float utility = 0.0f;

        if (max - min != 0)
        {
            utility = (float)(value - min) / (max - min);
        }

        utility *= Weight;
        Debug.Assert(utility >= 0.0f && utility <= 1.0f);
        return utility;
    }

    // TODO much of this is shared between Ability and Movement Utility Functions now.
    // A common UtilityFunction parent makes sense now
    public virtual Control InstantiateSetUtilityControl()
    {
        WeightSetUtilityControl setUtilityControl = _setUtilityControlScene.Instantiate<WeightSetUtilityControl>();
        setUtilityControl.DisplayNameLabel.Text = _displayName;
        setUtilityControl.Slider.Value = Weight;
        setUtilityControl.Slider.ValueChanged += OnSetUtilityControlSliderValueChanged;

        return setUtilityControl;
    }

    protected void OnSetUtilityControlSliderValueChanged(double value)
    {
        Weight = (float)value;
    }
}
