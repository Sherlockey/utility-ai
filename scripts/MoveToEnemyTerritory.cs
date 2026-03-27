using Godot;

using System.Collections.Generic;

public partial class MoveToEnemyTerritory : MovementUtility
{
    public override float Evaluate(
        Vector2I evaluatedCoords,
        Dictionary<Vector2I, (int, int)> influenceMap,
        Combatant.Team sourceTeam)
    {
        // If positive, more enemy dominated. If negative, more ally dominated
        // float difference = influenceMap[evaluatedCoords].Item1 - influenceMap[evaluatedCoords].Item2;
        // difference *= Weight;
        // return (sourceTeam == Combatant.Team.Enemy) ? difference : -difference;
        return influenceMap[evaluatedCoords].Item1 * Weight;
    }
}
