using Godot;

using System.Collections.Generic;

public struct Decision(Vector2I moveLocation, IAbility ability, List<Combatant> targets, float utility)
{
    public Vector2I MoveLocation { get; private set; } = moveLocation;
    public IAbility Ability { get; private set; } = ability;
    public List<Combatant> Targets { get; private set; } = targets;
    public float Utility { get; private set; } = utility;
}
