using Godot;

using System;

public partial class Status : Node
{
    public event EventHandler Died;

    [Export]
    public Stats Stats;

    public int CurrentHealth { get; set; }

    public override void _Ready()
    {
        CurrentHealth = Stats.Health;
    }


    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Died?.Invoke(this, EventArgs.Empty);

            // TODO: testing only
            Node parent = GetParent();
            if (parent != null)
            {
                GD.Print(parent.Name + " died");
            }
        }
    }
}
