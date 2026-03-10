using Godot;

public partial class Attack : Node, IAbility
{
    public void Apply(Stats stats, Combatant[] targets)
    {
        int damage = stats.Attack * stats.Attack;
        foreach (Combatant target in targets)
        {
            target.Status.TakeDamage(damage);
        }
    }
}
