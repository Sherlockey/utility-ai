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

    private readonly Random _random = new();

    public override void _Ready()
    {
        CurrentHealth = Stats.Health;
        CurrentAccuracy = Stats.Accuracy;
        CurrentEvasion = Stats.Evasion;
    }

    public void ResolveAttack(Combatant attacker, Ability ability)
    {
        // TODO make sure the math below is logical. Especially around 0 or 100 values
        int hitPercent = ability.GetHitPercentNumerator() + attacker.Status.CurrentAccuracy;
        hitPercent = Mathf.Min(hitPercent, 100);
        int hitChance = hitPercent - CurrentEvasion;
        hitChance = Mathf.Max(hitChance, 0);
        int roll = _random.Next(0, 100);

        if (hitChance > 0 && hitChance >= roll) // Hit
        {
            TakeDamage(ability.GetDamage(attacker));
        }
        else // Miss
        {
            if (Owner is Combatant defender)
            {
                MessageLog.Get().Write(defender.DisplayName + " evaded");
            }
        }
    }

    // TODO this is a temporary calculation
    public int GetInfluence()
    {
        return Stats.Attack * Stats.Attack;
    }

    private void TakeDamage(int damage)
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
}
