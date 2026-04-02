using Godot;

using System.Collections.Generic;

public partial class Attack : Ability
{
    [Export]
    private int _range = 1;

    public override void Apply(Combatant user, List<Combatant> targets)
    {
        MessageLog.Get().Write(user.DisplayName + " used Attack");
        int damage = GetDamageCalculation(user);
        foreach (Combatant target in targets)
        {
            target.Status.TakeDamage(damage * _damagePercentNumerator / 100);
        }
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

    private int GetDamageCalculation(Combatant user)
    {
        return user.Stats.Attack * user.Stats.Attack;
    }
}
