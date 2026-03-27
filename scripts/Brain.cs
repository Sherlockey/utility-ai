using Godot;

using System;

using System.Collections.Generic;

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
            float accumulatedScore = 0.0f;
            float min = 0.0f;
            float max = 1.0f;
            foreach (Node node in _movementUtilityParent.GetChildren())
            {
                if (node is MovementUtility movementUtility)
                {
                    float score = movementUtility.Evaluate(coords, influenceMap, sourceTeam);
                    accumulatedScore += score;
                    if (score < min)
                    {
                        min = score;
                    }
                    if (score > max)
                    {
                        max = score;
                    }
                }
            }
            //Normalize then assign
            accumulatedScore = (accumulatedScore - min) / (max - min);
            if (scoreMap.ContainsKey(coords))
            {
                scoreMap[coords] += accumulatedScore;
            }
            else
            {
                scoreMap[coords] = accumulatedScore;
            }
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
