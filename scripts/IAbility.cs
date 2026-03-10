// TODO: probably make this an abstract class instead with a constructor
// so we can pass info in and construct any kind of ability

public interface IAbility
{
    public void Apply(Combatant[] targets);
}
