using Godot;

using System.Collections.Generic;
using System.Diagnostics;

public partial class MoveToFriendlyTerritory : MovementUtility
{
    public override float Evaluate(
        Vector2I evaluatedCoords,
        Dictionary<Vector2I, (int, int)> influenceMap,
        Combatant.Team sourceTeam)
    {
        float utility = 0.0f;

        int minAllyDifference = int.MaxValue;
        int maxAllyDifference = int.MinValue;
        foreach (Vector2I coords in influenceMap.Keys)
        {
            int enemyInfluence = influenceMap[coords].Item1;
            int allyInfluence = influenceMap[coords].Item2;
            int difference = allyInfluence - enemyInfluence;
            if (difference < minAllyDifference)
            {
                minAllyDifference = difference;
            }
            if (difference > maxAllyDifference)
            {
                maxAllyDifference = difference;
            }
        }
        int diff = influenceMap[evaluatedCoords].Item2 - influenceMap[evaluatedCoords].Item1;
        if (minAllyDifference < 0)
        {
            int amountToShift = Mathf.Abs(minAllyDifference);
            minAllyDifference += amountToShift;
            maxAllyDifference += amountToShift;
            diff += amountToShift;
            if (maxAllyDifference - minAllyDifference != 0)
            {
                utility = (float)(diff - minAllyDifference) / (maxAllyDifference - minAllyDifference);
            }
        }

        utility *= Weight;
        Debug.Assert(utility >= 0.0f && utility <= 1.0f);
        return utility;
    }
}
