using System;
using System.Collections.Generic;

public struct BattleEndInfo
{
    public bool IsVictory;
    public Dictionary<string, Dictionary<Type, MovementUtilityFunction>> MovementDict; // string is combatant DisplayName
    public Dictionary<string, Dictionary<Type, AbilityUtilityFunction>> AbilityDict; // string is combatant DisplayName

    public BattleEndInfo(bool isVictory, Dictionary<string, Dictionary<Type, MovementUtilityFunction>> movementDict, Dictionary<string, Dictionary<Type, AbilityUtilityFunction>> abilityDict) : this()
    {
        IsVictory = isVictory;
        MovementDict = movementDict;
        AbilityDict = abilityDict;
    }
}
