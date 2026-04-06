using Godot;

using System.Collections.Generic;

public partial class Slam : Ability
{
    [Export]
    private int _range = 0;

    public override async void Execute(Combatant user, List<Combatant> targets)
    {
        MessageLog.Get().Write(user.DisplayName + " uses Slam");
        await AnimateExecute(user, targets);
        PlaySFX();
        foreach (Combatant target in targets)
        {
            target.Status.ResolveAttack(user, this);
        }
        await AnimateReset(user);
    }

    public override bool IsInRange(Vector2I startTileCoords, Vector2I endTileCoords)
    {
        return startTileCoords == endTileCoords;
    }

    public override List<Combatant> CombatantsInAreaOfEffect(Vector2I startCoords)
    {
        List<Combatant> combatants = [];
        // NOTE could refactor the true below as this procedure is a copy of abstract method otherwise
        List<Vector2I> coordsInAOE = Utils.CoordsInDist(startCoords, _areaOfEffect - 1, true);
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
