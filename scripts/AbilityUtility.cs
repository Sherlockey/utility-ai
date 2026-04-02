using Godot;

using System.Collections.Generic;

public abstract partial class AbilityUtility : Node
{
    [Export(PropertyHint.Range, "0.0, 1.0")]
    public float Weight = 0.5f;

    public abstract float Evaluate(IAbility ability, Combatant user, List<Combatant> targets);
}
