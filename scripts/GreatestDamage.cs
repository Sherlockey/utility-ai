using Godot;

using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class GreatestDamage : AbilityUtility
{
    public override float Evaluate(IAbility ability, Combatant user, List<Combatant> targets)
    {
        float utility = 0.0f;

        // Get max possible damage for this ability which is maxPossibleTargets * GetDamage()
        // Additive Factorial / Triangular Number = n * (n-1) / 2
        int aoe = ability.GetAreaOfEffect();
        int additiveFactorial = aoe * (aoe - 1) / 2;
        int dirs = 4;
        int initial = 1;
        int maxPossibleTargets = dirs * additiveFactorial + initial;
        int damage = ability.GetDamage(user);
        int maxPossibleDamage = maxPossibleTargets * damage;

        int estimatedDamage = 0;
        foreach (Combatant target in targets)
        {
            if (target.MyTeam == user.MyTeam)
            {
                continue;
            }
            int evasion = target.Status.CurrentEvasion;
            int hitPercent = ability.GetHitPercentNumerator() + user.Status.CurrentAccuracy;
            hitPercent = Mathf.Min(hitPercent, 100);
            estimatedDamage += damage * hitPercent / 100;
        }

        if (maxPossibleDamage != 0)
        {
            utility = (float)estimatedDamage / maxPossibleDamage;
        }

        utility *= Weight;
        Debug.Assert(utility >= 0.0f && utility <= 1.0f);
        return utility;
    }
}
