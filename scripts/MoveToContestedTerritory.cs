using Godot;

using System.Collections.Generic;
using System.Diagnostics;

public partial class MoveToContestedTerritory : MovementUtility
{
    public override float Evaluate(
        Vector2I evaluatedCoords,
        Dictionary<Vector2I, (int, int)> influenceMap,
        Combatant.Team sourceTeam)
    {
        float utility = 0.0f;

        int minScore = int.MaxValue;
        int maxScore = 0;
        foreach (Vector2I coords in influenceMap.Keys)
        {
            int enemyInfluence = influenceMap[evaluatedCoords].Item1;
            int allyInfluence = influenceMap[evaluatedCoords].Item2;
            int currSum = enemyInfluence + allyInfluence;
            int currDiff = Mathf.Abs(influenceMap[coords].Item1 - influenceMap[coords].Item2);
            int currScore = currSum - currDiff;
            if (currScore < minScore)
            {
                minScore = currScore;
            }
            if (currScore > maxScore)
            {
                maxScore = currScore;
            }
        }
        int sum =
            influenceMap[evaluatedCoords].Item1 + influenceMap[evaluatedCoords].Item2;
        int diff =
            Mathf.Abs(influenceMap[evaluatedCoords].Item1 - influenceMap[evaluatedCoords].Item2);
        int score = sum - diff;
        if (maxScore - minScore != 0)
        {
            if (sum == 0)
            {
                utility = 0.0f;
            }
            else
            {
                utility = ((float)score - minScore) / (maxScore - minScore);
            }
        }
        utility *= Weight;
        Debug.Assert(utility >= 0.0f && utility <= 1.0f);
        return utility;
    }
}
