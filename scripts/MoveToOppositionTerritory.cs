using Godot;

using System.Collections.Generic;
using System.Diagnostics;

public partial class MoveToOppositionTerritory : MovementUtilityFunction
{
    public override float CalculateUtility(
        Vector2I evaluatedCoords,
        Dictionary<Vector2I, (int, int)> influenceMap,
        Combatant.Team sourceTeam)
    {
        float utility = 0.0f;

        int minEnemyDifference = int.MaxValue;
        int maxEnemyDifference = int.MinValue;
        foreach (Vector2I coords in influenceMap.Keys)
        {
            int enemyInfluence = influenceMap[coords].Item1;
            int allyInfluence = influenceMap[coords].Item2;
            int difference = enemyInfluence - allyInfluence;
            if (difference < minEnemyDifference)
            {
                minEnemyDifference = difference;
            }
            if (difference > maxEnemyDifference)
            {
                maxEnemyDifference = difference;
            }
        }
        int diff = influenceMap[evaluatedCoords].Item1 - influenceMap[evaluatedCoords].Item2;
        if (minEnemyDifference < 0)
        {
            int amountToShift = Mathf.Abs(minEnemyDifference);
            minEnemyDifference += amountToShift;
            maxEnemyDifference += amountToShift;
            diff += amountToShift;
            if (maxEnemyDifference - minEnemyDifference != 0)
            {
                utility = (float)(diff - minEnemyDifference) / (maxEnemyDifference - minEnemyDifference);
            }
        }
        utility *= Weight;
        Debug.Assert(utility >= 0.0f && utility <= 1.0f);
        return utility;
    }
}
