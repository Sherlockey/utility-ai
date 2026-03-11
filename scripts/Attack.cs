using Godot;

public partial class Attack : Node, IAbility
{
    [Export]
    private int _range = 1;
    [Export]
    private int _areaOfEffect = 1;

    public void Apply(Combatant user, Combatant[] targets)
    {
        int damage = user.Stats.Attack * user.Stats.Attack;
        foreach (Combatant target in targets)
        {
            target.Status.TakeDamage(damage);
        }
    }

    public bool IsInRange(Vector2I startTileCoords, Vector2I endTileCoords)
    {
        int rangeX = Mathf.Abs(endTileCoords.X - startTileCoords.X);
        int rangeY = Mathf.Abs(endTileCoords.Y - startTileCoords.Y);
        int rangeSum = rangeX + rangeY;
        return rangeSum <= _range;
    }

    public bool IsTargetValid(Combatant user, Combatant target)
    {
        return target != user;
    }
}
