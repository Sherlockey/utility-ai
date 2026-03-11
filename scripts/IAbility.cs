using Godot;

public interface IAbility
{
    public void Apply(Combatant user, Combatant[] targets);
    public bool IsTargetValid(Combatant user, Combatant target);
    public bool IsInRange(Vector2I startTileCoords, Vector2I endTileCoords);
}
