using Godot;

using System;

public partial class Status : Node
{
    public event EventHandler<Combatant> Died;
    public event EventHandler<Combatant> DamageTaken;

    [Export]
    public Stats Stats;

    public int CurrentMovement;
    public int AccumulatedSpeed;
    public int CurrentHealth = 1;
    public int CurrentAccuracy;
    public int CurrentEvasion;
    public int AbilitiesRemaining; // TODO @Incomplete

    [Export]
    private double _hitDuration = 0.025;
    [Export]
    private double _resetDuration = 0.1;

    private string _evadeAudioStreamPath = "res://third-party/sfx/punch-air.mp3";
    private AudioStream _evadeAudioStream = null;

    private readonly Random _random = new();

    public override void _Ready()
    {
        _evadeAudioStream = GD.Load<AudioStream>(_evadeAudioStreamPath);

        ResetForNewBattle();
    }

    public void ResetForNewBattle()
    {
        CurrentHealth = Stats.Health;
        AccumulatedSpeed = 0;
        CurrentMovement = Stats.Movement;
        CurrentAccuracy = Stats.Accuracy;
        CurrentEvasion = Stats.Evasion;
        AbilitiesRemaining = Stats.AbilitiesPerTurn;
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
            TakeDamage(attacker, ability.GetDamage(attacker));
        }
        else // Miss
        {
            if (Owner is Combatant defender)
            {
                if (_evadeAudioStream != null)
                {
                    BattleManager battleManager = BattleManager.Get();
                    battleManager.AbilitySFXPlayer.Stream = _evadeAudioStream;
                    battleManager.AbilitySFXPlayer.Play();
                }

                MessageLog.Get().Write(defender.DisplayName + " evaded");
            }
        }
    }

    // TODO this is a temporary calculation
    public int GetInfluence()
    {
        return Stats.Attack * Stats.Attack;
    }

    private void TakeDamage(Combatant attacker, int damage)
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
            }
            else
            {
                AnimateTakeDamage(attacker);
            }
        }
    }

    private async void AnimateTakeDamage(Combatant attacker)
    {
        if (Owner is Combatant combatant)
        {
            TileMapLayer tileMapLayer = BattleManager.Get().TileMapLayer;
            Vector2I coords = tileMapLayer.LocalToMap(combatant.Position);
            Vector2I attackerCoords = tileMapLayer.LocalToMap(attacker.Position);
            int x = coords.X - attackerCoords.X;
            int y = coords.Y - attackerCoords.Y;
            x = Mathf.Clamp(x, -1, 1);
            y = Mathf.Clamp(y, -1, 1);

            Tween hitTween = GetTree().CreateTween();
            hitTween.TweenProperty(
                combatant.Sprite2D,
                "offset",
                new Vector2(x, y),
                _hitDuration);
            await ToSignal(hitTween, Tween.SignalName.Finished);

            Tween resetTween = GetTree().CreateTween();
            resetTween.TweenProperty(
                combatant.Sprite2D,
                "offset",
                Vector2.Zero,
                _resetDuration);
            await ToSignal(resetTween, Tween.SignalName.Finished);
        }
    }
}
