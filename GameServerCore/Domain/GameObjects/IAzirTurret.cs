namespace GameServerCore.Domain.GameObjects
{
    public interface IAzirTurret : IBaseTurret
    {
        IAttackableUnit Owner { get; }
    }
}
