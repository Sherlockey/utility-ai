using Godot;

using System.Collections.Generic;

public partial class Backstab : Ability
{
    [Export]
    private int _range = 1;

    public override async void Execute(Combatant user, List<Combatant> targets)
    {
        MessageLog.Get().Write(user.DisplayName + " uses Backstab");
        await AnimateExecute(user, targets);
        foreach (Combatant target in targets)
        {
            target.Status.ResolveAttack(user, this);
        }
        await AnimateReset(user);
    }

    public override bool IsInRange(Vector2I startTileCoords, Vector2I endTileCoords)
    {
        if (startTileCoords == endTileCoords)
        {
            return false;
        }
        int rangeX = Mathf.Abs(endTileCoords.X - startTileCoords.X);
        int rangeY = Mathf.Abs(endTileCoords.Y - startTileCoords.Y);
        int rangeSum = rangeX + rangeY;
        return rangeSum <= _range;
    }

    public override List<Combatant> ValidatedTargets(Combatant user, List<Combatant> targets)
    {
        List<Combatant> result = [];
        foreach (Combatant target in targets)
        {
            Vector2I targetCoords = BattleManager.Get().TileMapLayer.LocalToMap(target.Position);
            List<Vector2I> coordsAdjacentToTarget =
                Utils.CoordsInDist(targetCoords, 1, true);
            foreach (Combatant combatant in BattleManager.Get().Combatants)
            {
                Vector2I combatantCoords = BattleManager.Get().TileMapLayer.LocalToMap(combatant.Position);
                if (coordsAdjacentToTarget.Contains(combatantCoords)
                    && combatant.MyTeam != target.MyTeam
                    && combatant != user)
                {
                    result.Add(target);
                }
            }
        }
        return result;
    }
}
