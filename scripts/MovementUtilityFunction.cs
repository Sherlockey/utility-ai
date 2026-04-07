using Godot;

using System.Collections.Generic;

public abstract partial class MovementUtilityFunction : Node
{
    [Export(PropertyHint.Range, "0.0, 1.0")]
    public float Weight = 0.5f;

    public abstract float CalculateUtility(
        Vector2I evaluatedCoords,
        Dictionary<Vector2I, (int, int)> influenceMap,
        Combatant.Team sourceTeam);
}
