using Godot;

using System.Collections.Generic;

public abstract partial class Ability : Node, IAbility
{
    [Export]
    protected int _areaOfEffect = 1;

    public abstract void Apply(Combatant user, List<Combatant> targets);

    public abstract bool IsInRange(Vector2I startTileCoords, Vector2I endTileCoords);

    public virtual List<Combatant> CombatantsInAreaOfEffect(Vector2I startCoords)
    {
        List<Combatant> combatants = [];
        List<Vector2I> coordsInAOE = Utils.CoordsInDist(startCoords, _areaOfEffect - 1);
        foreach (Combatant combatant in BattleManager.Get().Combatants)
        {
            foreach (Vector2I coords in coordsInAOE)
            {
                if (BattleManager.Get().TileMapLayer.LocalToMap(combatant.Position) == coords)
                {
                    combatants.Add(combatant);
                }
            }
        }
        return combatants;
    }
}
