using Godot;

using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class AvoidFriendlyDamage : AbilityUtility
{
    public override float Evaluate(IAbility ability, Combatant user, List<Combatant> targets)
    {
        float utility = 0.0f;

        utility *= Weight;
        Debug.Assert(utility >= 0.0f && utility <= 1.0f);
        return utility;
    }
}
