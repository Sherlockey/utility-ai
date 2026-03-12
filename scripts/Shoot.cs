using Godot;

using System.Collections.Generic;

public partial class Shoot : Ability
{
    [Export]
    private int _minRange = 3;
    [Export]
    private int _maxRange = 5;

    public override void Apply(Combatant user, List<Combatant> targets)
    {
        int damage = user.Stats.Attack * user.Stats.Attack;
        foreach (Combatant target in targets)
        {
            target.Status.TakeDamage(damage);
        }
    }

    public override bool IsInRange(Vector2I startTileCoords, Vector2I endTileCoords)
    {
        int rangeX = Mathf.Abs(endTileCoords.X - startTileCoords.X);
        int rangeY = Mathf.Abs(endTileCoords.Y - startTileCoords.Y);
        int rangeSum = rangeX + rangeY;
        return rangeSum >= _minRange && rangeSum <= _maxRange;
    }
}
