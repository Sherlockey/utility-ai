using Godot;

using System;

public partial class Stats : Node
{
    [Export]
    public int BaseLevel = 1;

    [Export]
    private int _baseHealth = 44;
    [Export]
    private int _baseAttack = 4;
    [Export]
    private int _baseSpeed = 6;
    [Export]
    private int _baseMovement = 4;
    [Export]
    private int _baseAccuracy = 0;
    [Export]
    private int _baseEvasion = 10;
    [Export]
    private int _baseAbilitiesPerTurn = 1;
    [Export]
    private int _baseExperiencePoints = 0;
    [Export]
    private int _baseKnowledgePoints = 0;

    [Export]
    private float _healthPerLevelScalar = 0.2f;
    [Export]
    private float _attackPerLevelScalar = 0.2f;
    [Export]
    private float _speedPerLevelScalar = 0.1f;
    [Export]
    private float _movementPerLevelScalar = 0.0f;
    [Export]
    private float _accuracyPerLevelScalar = 0.0f;
    [Export]
    private float _evasionPerLevelScalar = 0.0f;

    public int Health { get; private set; }
    public int Attack { get; private set; }
    public int Speed { get; private set; }
    public int Movement { get; private set; }
    public int Accuracy { get; private set; }
    public int Evasion { get; private set; }
    public int AbilitiesPerTurn { get; private set; }

    public int Level { get; private set; }
    public int ExperiencePoints { get; private set; }
    public int KnowledgePoints { get; private set; }

    private const int ExperienceToLevel = 100;

    public override void _Ready()
    {
        Health = (int)(_baseHealth + _baseHealth * _healthPerLevelScalar * (BaseLevel - 1));
        Attack = (int)(_baseAttack + _baseAttack * _attackPerLevelScalar * (BaseLevel - 1));
        Speed = (int)(_baseSpeed + _baseSpeed * _speedPerLevelScalar * (BaseLevel - 1));
        Movement = (int)(_baseMovement + _baseMovement * _movementPerLevelScalar * (BaseLevel - 1));
        Accuracy = (int)(_baseAccuracy + _baseAccuracy * _accuracyPerLevelScalar * (BaseLevel - 1));
        Evasion = (int)(_baseEvasion + _baseEvasion * _evasionPerLevelScalar * (BaseLevel - 1));
        AbilitiesPerTurn = _baseAbilitiesPerTurn;
        Level = BaseLevel;
        ExperiencePoints = _baseExperiencePoints;
        KnowledgePoints = _baseKnowledgePoints;
    }

    public void ApplyExperiencePoints(int value)
    {
        ExperiencePoints += value;
        while (ExperiencePoints >= ExperienceToLevel)
        {
            ApplyLevelUp();
            ExperiencePoints %= ExperienceToLevel;
        }
    }

    public void ApplyLevelUp()
    {
        Level++;
        Health = (int)(_baseHealth + _baseHealth * _healthPerLevelScalar * (BaseLevel - 1));
        Attack = (int)(_baseAttack + _baseAttack * _attackPerLevelScalar * (BaseLevel - 1));
        Speed = (int)(_baseSpeed + _baseSpeed * _speedPerLevelScalar * (BaseLevel - 1));
        Movement = (int)(_baseMovement + _baseMovement * _movementPerLevelScalar * (BaseLevel - 1));
        Accuracy = (int)(_baseAccuracy + _baseAccuracy * _accuracyPerLevelScalar * (BaseLevel - 1));
        Evasion = (int)(_baseEvasion + _baseEvasion * _evasionPerLevelScalar * (BaseLevel - 1));
    }
}
