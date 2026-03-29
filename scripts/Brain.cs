using Godot;

using System;

using System.Collections.Generic;
using System.Diagnostics;

public partial class Brain : Node
{
    [Export]
    private Node _movementUtilityParent;

    public Vector2I ChooseMoveLocation(
        Vector2I sourceCoords,
        List<Vector2I> reachableCoords,
        Dictionary<Vector2I, (int, int)> influenceMap,
        Combatant.Team sourceTeam)
    {
        Vector2I result = sourceCoords;
        Dictionary<Vector2I, float> scoreMap = [];

        // Calculate utility for all reachableCoords
        foreach (Vector2I coords in reachableCoords)
        {
            float totalScore = 0.0f;
            float totalWeight = 0.0f;
            foreach (Node node in _movementUtilityParent.GetChildren())
            {
                if (node is MovementUtility movementUtility)
                {
                    float score = movementUtility.Evaluate(coords, influenceMap, sourceTeam);
                    totalScore += score;
                    totalWeight += movementUtility.Weight;
                }
            }
            // Assign
            if (totalWeight != 0.0f)
            {
                totalScore /= totalWeight;
            }
            Debug.Assert(totalScore <= 1.0f);
            scoreMap[coords] = totalScore;
        }

        // DEBUG Draw map to debug
        DebugTileMapLayer debugTileMapLayer = BattleManager.Get().DebugTileMapLayer;
        foreach (Vector2I coords in scoreMap.Keys)
        {
            byte green = (byte)Mathf.RoundToInt(scoreMap[coords] * 255.0f);
            Color color = Color.Color8(0, green, 0);
            debugTileMapLayer.SetCellModulate(coords, color);
        }

        // Extract max utility score in scoreMap and set result to the associated coords
        float maxScore = 0.0f;
        foreach (Vector2I coords in scoreMap.Keys)
        {
            if (scoreMap[coords] > maxScore)
            {
                maxScore = scoreMap[coords];
                if (reachableCoords.Contains(coords))
                {
                    result = coords;
                }
            }
        }

        // Random random = new();
        // int randomIndex = random.Next(reachableCoords.Count);
        // result = reachableCoords[randomIndex];
        return result;
    }
}
