using Godot;

using System.Collections.Generic;

public partial class Cast : Ability
{
    [Export]
    private int _range = 4;

    public override void Apply(Combatant user, List<Combatant> targets)
    {
        MessageLog.Get().Write(user.DisplayName + " used Cast");
        foreach (Combatant target in targets)
        {
            target.Status.ResolveAttack(user, this);
        }
    }

    public override bool IsInRange(Vector2I startTileCoords, Vector2I endTileCoords)
    {
        int rangeX = Mathf.Abs(endTileCoords.X - startTileCoords.X);
        int rangeY = Mathf.Abs(endTileCoords.Y - startTileCoords.Y);
        int rangeSum = rangeX + rangeY;
        return rangeSum <= _range;
    }
}
