using Godot;

using System.Collections.Generic;

public struct Decision(Vector2I moveLocation, IAbility ability, List<Combatant> targets,
    Dictionary<AbilityUtilityFunction, (int, float)> functionValueUtilityDict, float movementUtility,
    float abilityUtility, float totalUtility)
{
    public Vector2I MoveLocation { get; private set; } = moveLocation;
    public IAbility Ability { get; private set; } = ability;
    public List<Combatant> Targets { get; private set; } = targets;
    // Tuple Item1 is Value of the ability according to AbilityUtility, Item2 is the same but utility
    public Dictionary<AbilityUtilityFunction, (int, float)> FunctionValueUtilityDict =
                                                            functionValueUtilityDict;
    public float MovementUtility = movementUtility;
    public float AbilityUtility = abilityUtility;
    public float TotalUtility = totalUtility;
}
