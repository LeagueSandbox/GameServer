namespace GameServerCore.Domain.GameObjects
{
    public interface IStatModifierSpeed : IStatModifier
    {
        float SlowResist { get; set; }
    }
}
