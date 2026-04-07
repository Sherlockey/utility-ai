using Godot;

using System.Collections.Generic;
using System.Diagnostics;

public abstract partial class AbilityUtilityFunction : Node
{
    [Export(PropertyHint.Range, "0.0, 1.0")]
    public float Weight = 0.5f;

    public abstract int CalculateValue(IAbility ability, Combatant user, List<Combatant> targets);

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
}
