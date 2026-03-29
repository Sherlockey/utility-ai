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

        int maxAllyDifference = 0;
        foreach (Vector2I coords in influenceMap.Keys)
        {
            int allyInfluence = influenceMap[evaluatedCoords].Item1;
            int enemyInfluence = influenceMap[evaluatedCoords].Item2;
            int difference = allyInfluence - enemyInfluence;
            if (difference < 0)
            {
                difference = 0;
            }
            if (difference > maxAllyDifference)
            {
                maxAllyDifference = difference;
            }
        }
        int diff = influenceMap[evaluatedCoords].Item1 - influenceMap[evaluatedCoords].Item2;
        if (maxAllyDifference != 0)
        {
            utility = diff / maxAllyDifference;
        }
        utility *= Weight;
        Debug.Assert(utility >= 0.0f && utility <= 1.0f);
        return utility;
    }
}
