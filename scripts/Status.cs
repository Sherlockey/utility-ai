using Godot;

using System;

public partial class Status : Node
{
    public event EventHandler<Combatant> Died;
    public event EventHandler<Combatant> DamageTaken;

    [Export]
    public Stats Stats;

    public int CurrentMovement;
    public int AccumulatedSpeed = 0;
    public int CurrentHealth;
    public int CurrentAccuracy;
    public int CurrentEvasion;
    public int AbilitiesRemaining = 0; // TODO @Incomplete

    public override void _Ready()
    {
        CurrentHealth = Stats.Health;
        CurrentAccuracy = Stats.Accuracy;
        CurrentEvasion = Stats.Evasion;
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
