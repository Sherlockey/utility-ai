using Godot;

using System.Collections.Generic;

public interface IAbility
{
    public void Apply(Combatant user, List<Combatant> targets);

    public bool IsInRange(Vector2I startTileCoords, Vector2I endTileCoords);

    public List<Combatant> CombatantsInAreaOfEffect(Vector2I startCoords);

    public List<Combatant> ValidatedTargets(Combatant user, List<Combatant> targets);

    public string GetDisplayName();
}
