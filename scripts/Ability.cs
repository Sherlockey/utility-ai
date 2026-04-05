using Godot;

using System.Collections.Generic;
using System.Threading.Tasks;

public abstract partial class Ability : Node, IAbility
{
    [Export]
    protected string _displayName = "Ability";
    [Export]
    protected int _areaOfEffect = 1;
    [Export]
    protected int _hitPercentNumerator = 100;
    [Export]
    protected int _damagePercentNumerator = 100;

    public abstract void Execute(Combatant user, List<Combatant> targets);

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

    public virtual List<Combatant> ValidatedTargets(Combatant user, List<Combatant> targets)
    {
        return targets;
    }

    public virtual string GetDisplayName()
    {
        return _displayName;
    }

    public virtual int GetDamage(Combatant user)
    {
        return user.Stats.Attack * user.Stats.Attack * _damagePercentNumerator / 100;
    }

    public virtual int GetDamagePercentNumerator()
    {
        return _damagePercentNumerator;
    }

    public virtual int GetHitPercentNumerator()
    {
        return _hitPercentNumerator;
    }

    public virtual int GetAreaOfEffect()
    {
        return _areaOfEffect;
    }

    protected virtual async Task AnimateExecute(Combatant user, List<Combatant> targets)
    {
        if (targets.Count == 0)
        {
            return;
        }

        // Determine direction to animate towards
        Vector2I userCoords = BattleManager.Get().TileMapLayer.LocalToMap(user.Position);
        int x = 0;
        int y = 0;
        foreach (Combatant target in targets)
        {
            Vector2I targetCoords = BattleManager.Get().TileMapLayer.LocalToMap(target.Position);
            x += targetCoords.X - userCoords.X;
            y += targetCoords.Y - userCoords.Y;
        }
        x = Mathf.Clamp(x, -1, 1);
        y = Mathf.Clamp(y, -1, 1);

        if (x == 0 && y == 0)
        {
            Vector2I targetCoords = BattleManager.Get().TileMapLayer.LocalToMap(targets[0].Position);
            x = targetCoords.X - userCoords.X;
            y = targetCoords.Y - userCoords.Y;
            x = Mathf.Clamp(x, -1, 1);
            y = Mathf.Clamp(y, -1, 1);
            Tween useTween = GetTree().CreateTween();
            useTween.TweenProperty(user.Sprite2D, "offset", new Vector2(2.0f * x, 2.0f * y), 0.05);
            await ToSignal(useTween, Tween.SignalName.Finished);
        }
        else
        {
            Tween useTween = GetTree().CreateTween();
            useTween.TweenProperty(user.Sprite2D, "offset", new Vector2(2.0f * x, 2.0f * y), 0.05);
            await ToSignal(useTween, Tween.SignalName.Finished);
        }
    }

    protected virtual async Task AnimateReset(Combatant user)
    {
        Tween resetTween = GetTree().CreateTween();
        resetTween.TweenProperty(user.Sprite2D, "offset", Vector2.Zero, 0.1);
        await ToSignal(resetTween, Tween.SignalName.Finished);
    }
}
