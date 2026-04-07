using Godot;

using System.Collections.Generic;
using System.Diagnostics;

public partial class MoveToEnemyTerritory : MovementUtilityFunction
{
    public override float CalculateUtility(
        Vector2I evaluatedCoords,
        Dictionary<Vector2I, (int, int)> influenceMap,
        Combatant.Team sourceTeam)
    {
        int totalEnemyInfluence = 0;
        foreach (Combatant combatant in BattleManager.Get().Combatants)
        {
            if (combatant.MyTeam == Combatant.Team.Enemy)
            {
                totalEnemyInfluence += combatant.Status.GetInfluence();
            }
        }
        float utility = 0.0f;
        if (totalEnemyInfluence != 0)
        {
            utility = (float)influenceMap[evaluatedCoords].Item1 / totalEnemyInfluence;
        }
        utility *= Weight;
        Debug.Assert(utility >= 0.0f && utility <= 1.0f);
        return utility;
    }
}
