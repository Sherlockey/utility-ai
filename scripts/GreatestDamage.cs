using Godot;

using System.Collections.Generic;

public partial class GreatestDamage : AbilityUtility
{
    // Calculate the simulated damage this usage of ability will do.
    public override int CalculateValue(IAbility ability, Combatant user, List<Combatant> targets)
    {
        int value = 0;

        int damage = ability.GetDamage(user);
        foreach (Combatant target in targets)
        {
            int evasion = target.Status.CurrentEvasion;
            int hitNumerator = ability.GetHitPercentNumerator() + user.Status.CurrentAccuracy;
            hitNumerator = Mathf.Clamp(hitNumerator, 0, 100);

            if (target.MyTeam == user.MyTeam)
            {
                value -= damage * hitNumerator / 100;
            }
            else
            {
                value += damage * hitNumerator / 100;
            }
        }

        return value;
    }

    // public override float Evaluate(int min, int max, int value)
    // {
    //     float utility = 0.0f;

    //     if (min - max != 0)
    //     {
    //         utility = (float)(value - min) / (max - min);
    //     }

    //     utility *= Weight;
    //     Debug.Assert(utility >= 0.0f && utility <= 1.0f);
    //     return utility;

    // float utility = 0.0f;

    // if (targets.Count == 0)
    // {
    //     return utility;
    // }

    // // Get max possible damage for this ability which is maxPossibleTargets * GetDamage()
    // // Additive Factorial / Triangular Number = n * (n-1) / 2
    // int aoe = ability.GetAreaOfEffect();
    // int additiveFactorial = aoe * (aoe - 1) / 2;
    // int dirs = 4;
    // int initial = 1;
    // int maxPossibleTargets = dirs * additiveFactorial + initial;
    // int damage = ability.GetDamage(user);
    // int maxPossibleDamage = maxPossibleTargets * damage;

    // int estimatedDamage = 0;
    // foreach (Combatant target in targets)
    // {
    //     if (target.MyTeam == user.MyTeam)
    //     {
    //         continue;
    //     }
    //     int evasion = target.Status.CurrentEvasion;
    //     int hitNumerator = ability.GetHitPercentNumerator() + user.Status.CurrentAccuracy;
    //     hitNumerator = Mathf.Clamp(hitNumerator, 0, 100);
    //     estimatedDamage += damage * hitNumerator / 100;
    // }

    // if (maxPossibleDamage != 0)
    // {
    //     utility = (float)estimatedDamage / maxPossibleDamage;
    // }

    // utility *= Weight;
    // Debug.Assert(utility >= 0.0f && utility <= 1.0f);
    // return utility;
    // }
}
