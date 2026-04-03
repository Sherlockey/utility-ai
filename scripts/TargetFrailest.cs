using Godot;

using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class TargetFrailest : AbilityUtility
{
    // Get max health of every opposition; highest is least desirable, least is most desirable.
    // Foreach target in targets get utility of making them the target.
    // Sum those utilities. Divide by # of targets
    public override float Evaluate(IAbility ability, Combatant user, List<Combatant> targets)
    {
        float utility = 0.0f;

        int minHealth = int.MaxValue;
        int maxHealth = int.MinValue;
        foreach (Combatant combatant in BattleManager.Get().Combatants)
        {
            if (combatant.MyTeam != user.MyTeam)
            {
                if (combatant.Stats.Health < minHealth)
                {
                    minHealth = combatant.Stats.Health;
                }
                if (combatant.Stats.Health > maxHealth)
                {
                    maxHealth = combatant.Stats.Health;
                }
            }
        }

        foreach (Combatant target in targets)
        {
            if (target.MyTeam == user.MyTeam)
            {
                continue;
            }
            if (target.Stats.Health == minHealth)
            {
                utility += 1.0f;
            }
            else if (maxHealth - minHealth != 0.0f)
            {
                utility += (maxHealth - target.Stats.Health) / (maxHealth - minHealth);
            }
            else // if maxHealth is the same as minHealth then all targets are equally good
            {
                utility += 1.0f;
            }
        }
        if (targets.Count != 0)
        {
            utility /= targets.Count;
        }

        utility *= Weight;
        Debug.Assert(utility >= 0.0f && utility <= 1.0f);
        return utility;
    }
}
