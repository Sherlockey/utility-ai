using Godot;

using System.Collections.Generic;
using System.Diagnostics;

public partial class RangeFromOpposition : MovementUtility
{
    [Export(PropertyHint.Range, "0, 2147483647")]
    public int Range = 1;

    private const int BoostAmount = -5;

    public override float Evaluate(
        Vector2I evaluatedCoords,
        Dictionary<Vector2I, (int, int)> influenceMap,
        Combatant.Team sourceTeam)
    {
        float utility = 0.0f;

        // Fill differenceMap with accumulated differences between dist and Range
        Dictionary<Vector2I, int> differenceMap = [];
        BattleManager battleManager = BattleManager.Get();
        foreach (Combatant combatant in battleManager.Combatants)
        {
            if (combatant.MyTeam != sourceTeam)
            {
                Vector2I source = battleManager.TileMapLayer.LocalToMap(combatant.Position);
                (Dictionary<Vector2I, int>, Dictionary<Vector2I, Vector2I>) distPrev =
                    Utils.WalkableCoordsDistAndPrev(source, int.MaxValue, combatant.MyTeam, false, false);
                foreach (Vector2I coords in battleManager.TileMapLayer.GetUsedCells())
                {
                    if (distPrev.Item1.TryGetValue(coords, out int walkDistance))
                    {
                        int difference = Mathf.Abs(walkDistance - Range);
                        // Adding a boost to incentivize exact Range
                        if (difference == 0)
                        {
                            difference = BoostAmount;
                        }
                        if (differenceMap.ContainsKey(coords))
                        {
                            differenceMap[coords] += difference;
                        }
                        else
                        {
                            differenceMap[coords] = difference;
                        }
                    }
                }
            }
        }

        // Extract min and max values
        int minDifference = int.MaxValue;
        int maxDifference = int.MinValue;
        foreach (int value in differenceMap.Values)
        {
            if (value < minDifference)
            {
                minDifference = value;
            }
            if (value > maxDifference)
            {
                maxDifference = value;
            }
        }

        // Calculate utility given min and max
        if (differenceMap.TryGetValue(evaluatedCoords, out int diff))
        {
            // Min might be negative, shift up if so
            if (minDifference < 0)
            {
                int amountToShift = Mathf.Abs(minDifference);
                minDifference += amountToShift;
                maxDifference += amountToShift;
                diff += amountToShift;
            }
            if (maxDifference - minDifference != 0)
            {
                utility = ((float)(maxDifference - diff)) / (maxDifference - minDifference);
            }
        }
        utility *= Weight;
        Debug.Assert(utility >= 0.0f && utility <= 1.0f);
        return utility;
    }
}
