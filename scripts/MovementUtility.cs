using Godot;

using System.Collections.Generic;

public abstract partial class MovementUtility : Node
{
    [Export(PropertyHint.Range, "0.0, 1.0")]
    public float Weight { get; private set; } = 0.5f;

    public abstract float Evaluate(
        Vector2I evaluatedCoords,
        Dictionary<Vector2I, (int, int)> influenceMap,
        Combatant.Team sourceTeam);
}
