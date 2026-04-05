using System;
using System.Collections.Generic;

public struct BattleEndInfo
{
    public bool IsVictory;
    public Dictionary<string, Dictionary<Type, MovementUtility>> MovementDict; // string is combatant DisplayName
    public Dictionary<string, Dictionary<Type, AbilityUtility>> AbilityDict; // string is combatant DisplayName

    public BattleEndInfo(bool isVictory, Dictionary<string, Dictionary<Type, MovementUtility>> movementDict, Dictionary<string, Dictionary<Type, AbilityUtility>> abilityDict) : this()
    {
        IsVictory = isVictory;
        MovementDict = movementDict;
        AbilityDict = abilityDict;
    }
}
