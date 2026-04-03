using Godot;

using System;
using System.Collections;

using System.Collections.Generic;
using System.Diagnostics;

public partial class Brain : Node
{
    public List<MovementUtility> MovementUtilities { get; private set; } = [];
    public List<AbilityUtility> AbilityUtilities { get; private set; } = [];

    // NOTE: The pair of utility weights must add up to 1.0
    private static readonly float MovementUtilityWeight = 0.2f;
    private static readonly float AbilityUtilityWeight = 1.0f - MovementUtilityWeight;

    [Export]
    private Node _movementUtilityParent;
    [Export]
    private Node _abilityUtilityParent;

    public override void _Ready()
    {
        foreach (Node node in _movementUtilityParent.GetChildren())
        {
            if (node is MovementUtility movementUtility)
            {
                MovementUtilities.Add(movementUtility);
            }
        }
        foreach (Node node in _abilityUtilityParent.GetChildren())
        {
            if (node is AbilityUtility abilityUtility)
            {
                AbilityUtilities.Add(abilityUtility);
            }
        }
    }

    // Returns a list of decisions that have been sorted by highest utility
    public List<Decision> MakeDecisionList(Combatant combatant)
    {
        //Gather walkable coords
        Vector2I sourceCoords = BattleManager.Get().TileMapLayer.LocalToMap(combatant.Position);
        combatant.DistPrevMaps = Utils.WalkableCoordsDistAndPrev(
            sourceCoords, combatant.Status.CurrentMovement, combatant.MyTeam);
        List<Vector2I> reachableCoords = [.. combatant.DistPrevMaps.Item1.Keys];
        reachableCoords.Add(sourceCoords);

        // Display walkable coords
        BattleManager battleManager = BattleManager.Get();
        foreach (KeyValuePair<Vector2I, int> kvp in combatant.DistPrevMaps.Item1)
        {
            battleManager.TileMapLayer.SetCell(kvp.Key, 4, new(0, 0), 0);
        }

        // Get utility for using each ability at each possible target coords within walkable coords
        List<Decision> abilityUtilityList = [];
        foreach (Vector2I reachedCoords in reachableCoords)
        {
            foreach (IAbility ability in combatant.Abilities)
            {
                List<Vector2I> coordsInRange = [];
                foreach (Vector2I coords in BattleManager.Get().TileMapLayer.GetUsedCells())
                {
                    if (ability.IsInRange(reachedCoords, coords))
                    {
                        coordsInRange.Add(coords);
                    }
                }
                foreach (Vector2I targetedCoords in coordsInRange)
                {
                    // TODO make a better method which doesn't require culling after getting targets?
                    List<Combatant> targets = ability.CombatantsInAreaOfEffect(targetedCoords);
                    targets = ability.ValidatedTargets(combatant, targets);
                    float utility = EvaluateAbilityUtility(ability, combatant, targets);
                    Decision decision = new(reachedCoords, ability, targets, utility);
                    abilityUtilityList.Add(decision);
                }
            }
        }

        // Get utility for all walkable coords
        Dictionary<Vector2I, float> movementUtilityMap = MakeMovementUtilityMap(
            sourceCoords, reachableCoords, BattleManager.Get().InfluenceMap, combatant.MyTeam);

        // Sum utility for ability and movement into sorted decisionList
        List<Decision> decisionUtilityList = [];
        foreach (Decision abilityDecision in abilityUtilityList)
        {
            Vector2I coords = abilityDecision.MoveLocation;
            float movementUtility = movementUtilityMap[coords] * MovementUtilityWeight;
            float abilityUtility = abilityDecision.Utility * AbilityUtilityWeight;
            float decisionUtility = movementUtility + abilityUtility;
            Debug.Assert(decisionUtility >= 0.0f && decisionUtility <= 1.0f);
            Decision decision = new(
                abilityDecision.MoveLocation, abilityDecision.Ability,
                abilityDecision.Targets, decisionUtility);
            decisionUtilityList.Add(decision);
        }

        decisionUtilityList.Sort((x, y) => y.Utility.CompareTo(x.Utility));
        return decisionUtilityList;
    }

    private Dictionary<Vector2I, float> MakeMovementUtilityMap(Vector2I sourceCoords, List<Vector2I> reachableCoords, Dictionary<Vector2I, (int, int)> influenceMap, Combatant.Team sourceTeam)
    {
        Dictionary<Vector2I, float> scoreMap = [];

        // Calculate utility for all reachableCoords
        foreach (Vector2I coords in reachableCoords)
        {
            float totalScore = 0.0f;
            float totalWeight = 0.0f;
            foreach (MovementUtility movementUtility in MovementUtilities)
            {
                float score = movementUtility.Evaluate(coords, influenceMap, sourceTeam);
                totalScore += score;
                totalWeight += movementUtility.Weight;
            }
            // Assign
            if (totalWeight != 0.0f)
            {
                totalScore /= totalWeight;
            }
            Debug.Assert(totalScore >= 0.0f && totalScore <= 1.0f);
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
        // float maxScore = 0.0f;
        // foreach (Vector2I coords in scoreMap.Keys)
        // {
        //     if (scoreMap[coords] > maxScore)
        //     {
        //         maxScore = scoreMap[coords];
        //         if (reachableCoords.Contains(coords))
        //         {
        //             result = coords;
        //         }
        //     }
        // }

        // Random random = new();
        // int randomIndex = random.Next(reachableCoords.Count);
        // result = reachableCoords[randomIndex];
        return scoreMap;
    }

    private float EvaluateAbilityUtility(IAbility ability, Combatant user, List<Combatant> targets)
    {
        float utility = 0.0f;
        float totalWeight = 0.0f;
        foreach (AbilityUtility abilityUtility in AbilityUtilities)
        {
            utility += abilityUtility.Evaluate(ability, user, targets);
            totalWeight += abilityUtility.Weight;
            // TODO double check the logic here
            float damageScalar = ability.GetDamagePercentNumerator() / 100.0f;
            utility *= damageScalar;
            totalWeight *= damageScalar;
        }
        if (totalWeight != 0.0f)
        {
            utility /= totalWeight;
        }
        Debug.Assert(utility >= 0.0f && utility <= 1.0f);
        return utility;
    }

    private class DecisionComparer : IComparer<Decision>
    {
        public int Compare(Decision decisionX, Decision decisionY)
        {
            return decisionX.Utility.CompareTo(decisionY.Utility);
        }
    }
}
