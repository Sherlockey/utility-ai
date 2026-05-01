using Godot;

using System;

public static class Settings
{
    public enum BattleSpeed
    {
        Pause, One, Two, Three,
    }

    public static BattleSpeed TheBattleSpeed = BattleSpeed.Pause;
}
