using Godot;

using System.Collections.Generic;
using System.Diagnostics;

public partial class MoveToFriendlyTerritory : MovementUtilityFunction
{
    public override float CalculateUtility(
        Vector2I evaluatedCoords,
        Dictionary<Vector2I, (int, int)> influenceMap,
        Combatant.Team sourceTeam)
    {
        float utility = 0.0f;

        int minAllyDifference = int.MaxValue;
        int maxAllyDifference = int.MinValue;
        foreach (Vector2I coords in influenceMap.Keys)
        {
            int oppositionInfluence = 0;
            int friendlyInfluence = 0;
            if (sourceTeam == Combatant.Team.Enemy)
            {
                oppositionInfluence = influenceMap[coords].Item2;
                friendlyInfluence = influenceMap[coords].Item1;
            }
            else if (sourceTeam == Combatant.Team.Ally)
            {
                oppositionInfluence = influenceMap[coords].Item1;
                friendlyInfluence = influenceMap[coords].Item2;
            }

            int difference = friendlyInfluence - oppositionInfluence;

            if (difference < minAllyDifference)
            {
                minAllyDifference = difference;
            }
            if (difference > maxAllyDifference)
            {
                maxAllyDifference = difference;
            }
        }

        int oppositionInfl = 0;
        int friendlyInfl = 0;
        if (sourceTeam == Combatant.Team.Enemy)
        {
            oppositionInfl = influenceMap[evaluatedCoords].Item2;
            friendlyInfl = influenceMap[evaluatedCoords].Item1;
        }
        else if (sourceTeam == Combatant.Team.Ally)
        {
            oppositionInfl = influenceMap[evaluatedCoords].Item1;
            friendlyInfl = influenceMap[evaluatedCoords].Item2;
        }

        int diff = friendlyInfl - oppositionInfl;
        if (maxAllyDifference - minAllyDifference != 0)
        {
            utility = (float)(diff - minAllyDifference) / (maxAllyDifference - minAllyDifference);
        }
        utility *= Weight;
        Debug.Assert(utility >= 0.0f && utility <= 1.0f);
        return utility;
    }
}
