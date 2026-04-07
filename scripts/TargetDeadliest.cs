using Godot;

using System.Collections.Generic;

public partial class TargetDeadliest : AbilityUtility
{
    public override int CalculateValue(IAbility ability, Combatant user, List<Combatant> targets)
    {
        int value = 0;

        foreach (Combatant target in targets)
        {
            int influence = target.Status.GetInfluence();
            if (target.MyTeam == user.MyTeam)
            {
                value -= influence;
            }
            else
            {
                value += influence;
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

    //     int minInfluence = int.MaxValue;
    //     int maxInfluence = int.MinValue;
    //     foreach (Combatant combatant in BattleManager.Get().Combatants)
    //     {
    //         if (combatant.MyTeam != user.MyTeam)
    //         {
    //             int influence = combatant.Status.GetInfluence();
    //             if (influence < minInfluence)
    //             {
    //                 minInfluence = influence;
    //             }
    //             if (influence > maxInfluence)
    //             {
    //                 maxInfluence = influence;
    //             }
    //         }
    //     }

    //     foreach (Combatant target in targets)
    //     {
    //         if (target.MyTeam == user.MyTeam)
    //         {
    //             continue;
    //         }
    //         int influence = target.Status.GetInfluence();
    //         if (maxInfluence - minInfluence != 0)
    //         {
    //             utility += (float)(influence - minInfluence) / (maxInfluence - minInfluence);
    //         }
    //         else
    //         {
    //             utility += 1.0f;
    //         }
    //     }
    //     if (targets.Count != 0)
    //     {
    //         utility /= targets.Count;
    //     }

    //     utility *= Weight;
    //     Debug.Assert(utility >= 0.0f && utility <= 1.0f);
    //     return utility;
    // }
}
