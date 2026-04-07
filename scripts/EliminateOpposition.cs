using Godot;

using System;
using System.Collections.Generic;

public partial class EliminateOpposition : AbilityUtilityFunction
{
    private const int EliminationBoostAmount = 50; // euivalent of half of a target's current health

    // Value is the numerator of the percent of health taken away plus elimination boosts
    public override int CalculateValue(IAbility ability, Combatant user, List<Combatant> targets)
    {
        int value = 0;
        int damage = ability.GetDamage(user);
        int hitNumerator = ability.GetHitPercentNumerator() + user.Status.CurrentAccuracy;

        foreach (Combatant target in targets)
        {
            int calc = 0;

            hitNumerator -= target.Status.CurrentEvasion;
            hitNumerator = Mathf.Clamp(hitNumerator, 0, 100);
            int estimatedDamage = damage * hitNumerator / 100;
            int currHealth = target.Status.CurrentHealth;

            calc = Mathf.Min(estimatedDamage, currHealth) * 100 / currHealth;
            if (estimatedDamage >= currHealth)
            {
                calc += EliminationBoostAmount;
            }
            if (target.MyTeam == user.MyTeam)
            {
                value -= calc;
            }
            else
            {
                value += calc;
            }
        }

        return value;
    }

    // public override float Evaluate(IAbility ability, Combatant user, List<Combatant> targets)
    // {
    //     float utility = 0.0f;

    //     if (targets.Count == 0)
    //     {
    //         return utility;
    //     }

    //     int damage = ability.GetDamage(user);
    //     int hitNumerator = ability.GetHitPercentNumerator() + user.Status.CurrentAccuracy;

    //     float eliminationBoostTotal = 0.0f;
    //     foreach (Combatant target in targets)
    //     {
    //         if (target.MyTeam == user.MyTeam)
    //         {
    //             continue;
    //         }

    //         hitNumerator -= target.Status.CurrentEvasion;
    //         hitNumerator = Mathf.Clamp(hitNumerator, 0, 100);
    //         int estimatedDamage = damage * hitNumerator / 100;
    //         int estimatedRemainingHealth = target.Status.CurrentHealth - estimatedDamage;
    //         estimatedRemainingHealth = Mathf.Max(estimatedRemainingHealth, 0);

    //         if (target.Status.CurrentHealth != 0) // "Impossible", but putting the check anyways
    //         {
    //             utility += (float)(target.Status.CurrentHealth - estimatedRemainingHealth) /
    //                 target.Status.CurrentHealth;
    //         }
    //         // Boost for actually eliminating an enemy
    //         if (estimatedRemainingHealth == 0)
    //         {
    //             utility += EliminationBoostAmount;
    //             eliminationBoostTotal += EliminationBoostAmount;
    //         }
    //     }
    //     utility -= eliminationBoostTotal;
    //     if (targets.Count != 0)
    //     {
    //         utility /= targets.Count;
    //     }

    //     utility *= Weight;
    //     Debug.Assert(utility >= 0.0f && utility <= 1.0f);
    //     return utility;
    // }
}
