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
    public int AbilitiesRemaining { get; set; } = 0; // TODO @Incomplete

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
            MessageLog.Get().Write(combatant.DisplayName + " took " + damage + " damage");
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Died?.Invoke(this, combatant);
                MessageLog.Get().Write(combatant.DisplayName + " was knocked out");
                // TODO this is temporary. Really we want to be able to resurrect fallen Combatants
                combatant.QueueFree();
            }
        }
    }

    // TODO this is a temporary calculation
    public int GetInfluence()
    {
        return Stats.Attack * Stats.Attack;
    }
}
