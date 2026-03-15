using Godot;

using System;

public partial class Status : Node
{
    public event EventHandler<Combatant> Died;
    public event EventHandler<Combatant> DamageTaken;

    [Export]
    public Stats Stats;

    public int CurrentMovement { get; set; }
    public int AccumulatedSpeed { get; set; } = 0;
    public int CurrentHealth { get; set; }
    public int AbilitiesRemaining { get; set; } = 0; // TODO this concept is @Incomplete

    public override void _Ready()
    {
        CurrentHealth = Stats.Health;
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        if (Owner is Combatant combatant)
        {
            DamageTaken?.Invoke(this, combatant);
        }
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            if (Owner is Combatant owner)
            {
                Died?.Invoke(this, owner);
                // TODO this is temporary. Really we want to be able to resurrect fallen Combatants
                Owner.QueueFree();
            }
        }
        // TODO testing only
        GD.Print(Owner.Name + " took " + damage + " damage and has " + CurrentHealth + " health remaining.");
    }
}
