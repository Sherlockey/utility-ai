using Godot;

using System;

public partial class Stats : Node
{
    [Export]
    public int BaseLevel = 1;
    [Export]
    public int ExperienceReward = 0;
    [Export]
    public int KnowledgeReward = 0;

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
    private float _healthPerLevelScalar = 0.1f;
    [Export]
    private float _attackPerLevelScalar = 0.05f;
    [Export]
    private float _speedPerLevelScalar = 0.015f;
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
        Level = BaseLevel; // Level must come first
        Health = (int)(_baseHealth * (1 + (_healthPerLevelScalar * (Level - 1))));
        Attack = (int)(_baseAttack * (1 + (_attackPerLevelScalar * (Level - 1))));
        Speed = (int)(_baseSpeed * (1 + (_speedPerLevelScalar * (Level - 1))));
        Movement = (int)(_baseMovement * (1 + (_movementPerLevelScalar * (Level - 1))));
        Accuracy = (int)(_baseAccuracy * (1 + (_accuracyPerLevelScalar * (Level - 1))));
        Evasion = (int)(_baseEvasion * (1 + (_evasionPerLevelScalar * (Level - 1))));
        AbilitiesPerTurn = _baseAbilitiesPerTurn;
        ExperiencePoints = _baseExperiencePoints;
        KnowledgePoints = _baseKnowledgePoints;
    }

    public void GainExperiencePoints(int value)
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
        GD.Print("Before level: " + Health + " " + Attack + " " + Speed + " " + Movement + " " + Accuracy + " " + Evasion);
        Level++; // Level must come first
        Health = (int)(_baseHealth * (1 + (_healthPerLevelScalar * (Level - 1))));
        Attack = (int)(_baseAttack * (1 + (_attackPerLevelScalar * (Level - 1))));
        Speed = (int)(_baseSpeed * (1 + (_speedPerLevelScalar * (Level - 1))));
        Movement = (int)(_baseMovement * (1 + (_movementPerLevelScalar * (Level - 1))));
        Accuracy = (int)(_baseAccuracy * (1 + (_accuracyPerLevelScalar * (Level - 1))));
        Evasion = (int)(_baseEvasion * (1 + (_evasionPerLevelScalar * (Level - 1))));
        GD.Print("After level: " + Health + " " + Attack + " " + Speed + " " + Movement + " " + Accuracy + " " + Evasion);
    }

    public void GainKnowledgePoints(int value)
    {
        KnowledgePoints += value;
    }
}
