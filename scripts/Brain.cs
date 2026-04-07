using Godot;

using System;

using System.Collections.Generic;
using System.Diagnostics;

public partial class Brain : Node
{
    public List<MovementUtilityFunction> MovementUtilities { get; private set; } = [];
    public List<AbilityUtilityFunction> AbilityUtilities { get; private set; } = [];

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
            if (node is MovementUtilityFunction movementUtility)
            {
                MovementUtilities.Add(movementUtility);
            }
        }
        foreach (Node node in _abilityUtilityParent.GetChildren())
        {
            if (node is AbilityUtilityFunction abilityUtility)
            {
                AbilityUtilities.Add(abilityUtility);
            }
        }
    }

    // Returns a list of decisions that have been sorted by highest utility
    public List<Decision> MakeDecisionList(Combatant combatant)
    {
        List<Decision> decisions = [];

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

        // Item1 = min, Item2 = max
        Dictionary<AbilityUtilityFunction, (int, int)> functionValueMinMax = [];
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
                    Decision decision = new(reachedCoords, ability, targets, [], 0.0f, 0.0f, 0.0f);
                    decisions.Add(decision);
                    foreach (AbilityUtilityFunction auf in AbilityUtilities)
                    {
                        int value = auf.CalculateValue(ability, combatant, targets);
                        decision.FunctionValueUtilityDict[auf] = (value, 0.0f);

                        if (!functionValueMinMax.ContainsKey(auf))
                        {
                            functionValueMinMax[auf] = (value, value);
                        }
                        if (value < functionValueMinMax[auf].Item1)
                        {
                            functionValueMinMax[auf] =
                                (value, functionValueMinMax[auf].Item2);
                        }
                        if (value > functionValueMinMax[auf].Item2)
                        {
                            functionValueMinMax[auf] =
                                (functionValueMinMax[auf].Item1, value);
                        }
                    }
                }
            }
        }

        // Every decision needs to have its values evaluated and given a utility based upon every abilityUtility
        foreach (Decision decision in decisions)
        {
            foreach (AbilityUtilityFunction auf in decision.FunctionValueUtilityDict.Keys)
            {
                int min = functionValueMinMax[auf].Item1;
                int max = functionValueMinMax[auf].Item2;
                int value = decision.FunctionValueUtilityDict[auf].Item1;
                float utility;
                if (max <= 0 || value <= 0)
                {
                    utility = 0.0f;
                }
                else
                {
                    utility = auf.CalculateUtility(min, max, value);
                }
                decision.FunctionValueUtilityDict[auf] = (value, utility);
            }
        }

        // Every decision needs to have its final abilityUtility evaluated by adding every score in functionValueUtilityDict and dividing by the totalWeight in the system
        for (int i = 0; i < decisions.Count; i++)
        {
            float decisionTotalWeight = 0.0f;
            float decisionAbilityUtility = 0.0f;
            foreach (KeyValuePair<AbilityUtilityFunction, (int, float)> kvp in
                decisions[i].FunctionValueUtilityDict)
            {
                decisionAbilityUtility += kvp.Value.Item2;
                decisionTotalWeight += kvp.Key.Weight;
            }
            if (decisionTotalWeight != 0.0f)
            {
                decisionAbilityUtility /= decisionTotalWeight;
            }
            Debug.Assert(decisionAbilityUtility >= 0.0f && decisionAbilityUtility <= 1.0f);
            Decision temp = decisions[i];
            temp.AbilityUtility = decisionAbilityUtility;
            decisions[i] = temp;
        }

        // Every decision needs to have its movement utility injected based upon each MoveLocation
        for (int i = 0; i < decisions.Count; i++)
        {
            Decision temp = decisions[i];
            temp.MovementUtility = movementUtilityMap[temp.MoveLocation];
            decisions[i] = temp;
        }

        // Sum utility for ability and movement into sorted decisionList
        for (int i = 0; i < decisions.Count; i++)
        {
            Vector2I coords = decisions[i].MoveLocation;
            float movementUtility = movementUtilityMap[coords] * MovementUtilityWeight;
            float abilityUtility = decisions[i].AbilityUtility * AbilityUtilityWeight;
            float totalUtility = movementUtility + abilityUtility;
            Debug.Assert(totalUtility >= 0.0f && totalUtility <= 1.0f);

            Decision temp = decisions[i];
            temp.TotalUtility = totalUtility;
            decisions[i] = temp;
        }

        decisions.Sort((x, y) => y.TotalUtility.CompareTo(x.TotalUtility));
        return decisions;
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
            float totalUtility = 0.0f;
            float totalWeight = 0.0f;
            foreach (MovementUtilityFunction movementUtility in MovementUtilities)
            {
                float score = movementUtility.CalculateUtility(coords, influenceMap, sourceTeam);
                totalUtility += score;
                totalWeight += movementUtility.Weight;
            }
            // Assign
            if (totalWeight != 0.0f)
            {
                totalUtility /= totalWeight;
            }
            Debug.Assert(totalUtility >= 0.0f && totalUtility <= 1.0f);
            scoreMap[coords] = totalUtility;
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
}
