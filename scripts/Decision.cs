using Godot;

using System.Collections.Generic;

public struct Decision(Vector2I moveLocation, IAbility ability, List<Combatant> targets,
    float movementUtility, float abilityUtility, float totalUtility)
{
    public Vector2I MoveLocation { get; private set; } = moveLocation;
    public IAbility Ability { get; private set; } = ability;
    public List<Combatant> Targets { get; private set; } = targets;
    public float MovementUtility { get; private set; } = movementUtility;
    public float AbilityUtility { get; private set; } = abilityUtility;
    public float TotalUtility { get; private set; } = totalUtility;
}
