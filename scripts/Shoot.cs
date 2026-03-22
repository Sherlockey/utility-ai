using Godot;

using System.Collections.Generic;
using System.Diagnostics;

public partial class Shoot : Ability
{
    [Export]
    private int _minRange = 3;
    [Export]
    private int _maxRange = 5;

    public override void Apply(Combatant user, List<Combatant> targets)
    {
        MessageLog.Get().Write(user.Name + " used Shoot");
        int damage = user.Stats.Attack * user.Stats.Attack;
        foreach (Combatant target in targets)
        {
            target.Status.TakeDamage(damage * _damagePercentNumerator / 100);
        }
    }

    public override bool IsInRange(Vector2I startTileCoords, Vector2I endTileCoords)
    {
        int rangeX = Mathf.Abs(endTileCoords.X - startTileCoords.X);
        int rangeY = Mathf.Abs(endTileCoords.Y - startTileCoords.Y);
        int rangeSum = rangeX + rangeY;
        return rangeSum >= _minRange && rangeSum <= _maxRange;
    }

    // Remove any targets that have a cell between the user and the target which blocks projectiles
    public override List<Combatant> ValidatedTargets(Combatant user, List<Combatant> targets)
    {
        List<Combatant> result = [];
        foreach (Combatant target in targets)
        {
            result.Add(target);
        }
        TileMapLayer tileMapLayer = BattleManager.Get().TileMapLayer;
        Vector2I startCoords = tileMapLayer.LocalToMap(user.Position);
        foreach (Combatant target in targets)
        {
            Vector2I endCoords = tileMapLayer.LocalToMap(target.Position);
            List<Vector2I> coordsTraversedToTarget = Utils.PlotLine(startCoords, endCoords);
            foreach (Vector2I coords in coordsTraversedToTarget)
            {
                string customDataLayerName = "BlocksProjectiles";
                Debug.Assert(tileMapLayer.GetCellTileData(coords).HasCustomData(customDataLayerName));
                if (tileMapLayer.GetCellTileData(coords).HasCustomData(customDataLayerName))
                {
                    bool doesBlockProjectiles =
                        tileMapLayer.GetCellTileData(coords).GetCustomData(customDataLayerName).AsBool();
                    if (doesBlockProjectiles)
                    {
                        result.Remove(target);
                        continue;
                    }
                }
            }
        }
        return result;
    }
}
