using Godot;

using System;

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
        // TODO this is repeated work in Combatant.InitializeTurn() and here
        //Gather walkable coords
        Vector2I sourceCoords = BattleManager.Get().TileMapLayer.LocalToMap(combatant.Position);
        combatant.DistPrevMaps = Utils.WalkableCoordsDistAndPrev(
            sourceCoords, combatant.Status.CurrentMovement, combatant.MyTeam);
        List<Vector2I> reachableCoords = [.. combatant.DistPrevMaps.Item1.Keys];
        reachableCoords.Add(sourceCoords);

        // TODO this is repeated work in Combatant.InitializeTurn() and here
        // Display walkable coords
        BattleManager battleManager = BattleManager.Get();
        foreach (KeyValuePair<Vector2I, int> kvp in combatant.DistPrevMaps.Item1)
        {
            battleManager.TileMapLayer.SetCell(kvp.Key, 4, new(0, 0), 0);
        }

        // Get utility for all walkable coords
        Dictionary<Vector2I, float> movementUtilityMap = MakeMovementUtilityMap(
            sourceCoords, reachableCoords, BattleManager.Get().InfluenceMap, combatant.MyTeam);

        // Get utility for using each ability at each possible target coords within walkable coords
        // V1
        // List<AbilityDecision> abilityDecisionList = [];
        // foreach (Vector2I reachedCoords in reachableCoords)
        // {
        //     foreach (IAbility ability in combatant.Abilities)
        //     {
        //         List<Vector2I> coordsInRange = [];
        //         foreach (Vector2I coords in BattleManager.Get().TileMapLayer.GetUsedCells())
        //         {
        //             if (ability.IsInRange(reachedCoords, coords))
        //             {
        //                 coordsInRange.Add(coords);
        //             }
        //         }
        //         foreach (Vector2I targetedCoords in coordsInRange)
        //         {
        //             // TODO make a better method which doesn't require culling after getting targets?
        //             List<Combatant> targets = ability.CombatantsInAreaOfEffect(targetedCoords);
        //             targets = ability.ValidatedTargets(combatant, targets);
        //             float utility = EvaluateAbilityUtility(ability, combatant, targets);
        //             AbilityDecision abilityDecision = new(reachedCoords, ability, targets, utility);
        //             abilityDecisionList.Add(abilityDecision);
        //         }
        //     }
        // }

        // V2
        Dictionary<AbilityUtility, (int, int)> functionValueMinMax = []; // Item1 = min, Item2 = max
        List<DecisionValue> decisionValues = [];
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
                    List<Combatant> targets = ability.CombatantsInAreaOfEffect(targetedCoords);
                    targets = ability.ValidatedTargets(combatant, targets);
                    foreach (AbilityUtility abilityUtility in AbilityUtilities)
                    {
                        int value = abilityUtility.CalculateValue(ability, combatant, targets);
                        DecisionValue entry =
                            new(abilityUtility, reachedCoords, ability, targets, value);
                        decisionValues.Add(entry);

                        if (!functionValueMinMax.ContainsKey(abilityUtility))
                        {
                            functionValueMinMax[abilityUtility] = (value, value);
                        }
                        if (value < functionValueMinMax[abilityUtility].Item1)
                        {
                            functionValueMinMax[abilityUtility] =
                                (value, functionValueMinMax[abilityUtility].Item2);
                        }
                        if (value > functionValueMinMax[abilityUtility].Item2)
                        {
                            functionValueMinMax[abilityUtility] =
                                (functionValueMinMax[abilityUtility].Item1, value);
                        }
                    }
                }
            }
        }

        // TODO this only looks at the highest utility score from any one utilityFunction,
        // need to sum them all for each decision and divide according to totalWeight

        // foreach AbilityUtility, score the utility of using that abiliity by comparing the
        // min and max values for any entry versus the actual value of that entry
        List<AbilityDecision> abilityDecisionList = [];
        foreach (DecisionValue dv in decisionValues)
        {
            int min = functionValueMinMax[dv.AbilityUtility].Item1;
            int max = functionValueMinMax[dv.AbilityUtility].Item2;
            float utility;
            if (max <= 0)
            {
                utility = 0.0f;
            }
            else
            {
                utility = dv.AbilityUtility.Evaluate(min, max, dv.Value);
            }
            AbilityDecision abilityDecision = new(dv.MoveLocation, dv.Ability, dv.Targets, utility);
            abilityDecisionList.Add(abilityDecision);
        }

        // Sum utility for ability and movement into sorted decisionList
        List<Decision> decisionUtilityList = [];
        foreach (AbilityDecision abilityDecision in abilityDecisionList)
        {
            Vector2I coords = abilityDecision.MoveLocation;
            float movementUtility = movementUtilityMap[coords] * MovementUtilityWeight;
            float abilityUtility = abilityDecision.Utility * AbilityUtilityWeight;
            float decisionUtility = movementUtility + abilityUtility;
            Debug.Assert(decisionUtility >= 0.0f && decisionUtility <= 1.0f);
            Decision decision = new(
                abilityDecision.MoveLocation, abilityDecision.Ability,
                abilityDecision.Targets, movementUtility, abilityDecision.Utility, decisionUtility);
            decisionUtilityList.Add(decision);
        }

        decisionUtilityList.Sort((x, y) => y.TotalUtility.CompareTo(x.TotalUtility));
        return decisionUtilityList;
    }

    private Dictionary<Vector2I, float> MakeMovementUtilityMap(
        Vector2I sourceCoords, List<Vector2I> reachableCoords,
        Dictionary<Vector2I, (int, int)> influenceMap,
        Combatant.Team sourceTeam)
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

        return scoreMap;
    }

    private struct DecisionValue(AbilityUtility abilityUtility, Vector2I moveLocation, IAbility ability, List<Combatant> targets, int value)
    {
        public AbilityUtility AbilityUtility { get; private set; } = abilityUtility;
        public Vector2I MoveLocation { get; private set; } = moveLocation;
        public IAbility Ability { get; private set; } = ability;
        public List<Combatant> Targets { get; private set; } = targets;
        public int Value { get; private set; } = value;
    }

    // private float EvaluateAbilityUtility(IAbility ability, Combatant user, List<Combatant> targets)
    // {
    //     float utility = 0.0f;
    //     float totalWeight = 0.0f;
    //     foreach (AbilityUtility abilityUtility in AbilityUtilities)
    //     {
    //         utility += abilityUtility.Evaluate(ability, user, targets);
    //         totalWeight += abilityUtility.Weight;
    //         // TODO double check the logic here
    //         float damageScalar = ability.GetDamagePercentNumerator() / 100.0f;
    //         utility *= damageScalar;
    //         totalWeight *= damageScalar;
    //     }
    //     if (totalWeight != 0.0f)
    //     {
    //         utility /= totalWeight;
    //     }
    //     Debug.Assert(utility >= 0.0f && utility <= 1.0f);
    //     return utility;
    // }
}
