using Godot;

using System.Collections.Generic;
using System.Diagnostics;

public partial class MoveToAllyTerritory : MovementUtility
{
    public override float Evaluate(
        Vector2I evaluatedCoords,
        Dictionary<Vector2I, (int, int)> influenceMap,
        Combatant.Team sourceTeam)
    {
        int totalAllyInfluence = 0;
        foreach (Combatant combatant in BattleManager.Get().Combatants)
        {
            if (combatant.MyTeam == Combatant.Team.Ally)
            {
                totalAllyInfluence += combatant.Status.GetInfluence();
            }
        }
        float utility = 0.0f;
        if (totalAllyInfluence != 0)
        {
            utility = (float)influenceMap[evaluatedCoords].Item2 / totalAllyInfluence;
        }
        // float utility = Curve.Sample(influenceMap[evaluatedCoords].Item2);
        utility *= Weight;
        Debug.Assert(utility >= 0.0f && utility <= 1.0f);
        return utility;
    }
}
