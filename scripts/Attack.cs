using Godot;

public partial class Attack : Node, IAbility
{
    // public Attack()
    // {
    //     _damage = 0;
    //     _accuracy = 100;
    //     _target = null;
    // }

    // public Attack(int damage, int accuracy, Combatant target)
    // {
    //     _damage = damage;
    //     _accuracy = accuracy;
    //     _target = target;
    // }

    public int _accuracy = 100;
    public int _damage;
    public Combatant _target;

    public void Apply()
    {
        Combatant coyena = GetNode<Combatant>("/root/TestLevel/Coyena");
        coyena.Status.TakeDamage(_damage * _damage);
    }
}