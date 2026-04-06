using Godot;

using System.Collections.Generic;

public partial class Attack : Ability
{
    [Export]
    private int _range = 1;

    public override async void Execute(Combatant user, List<Combatant> targets)
    {
        MessageLog.Get().Write(user.DisplayName + " uses Attack");
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
        if (startTileCoords == endTileCoords)
        {
            return false;
        }
        int rangeX = Mathf.Abs(endTileCoords.X - startTileCoords.X);
        int rangeY = Mathf.Abs(endTileCoords.Y - startTileCoords.Y);
        int rangeSum = rangeX + rangeY;
        return rangeSum <= _range;
    }
}
