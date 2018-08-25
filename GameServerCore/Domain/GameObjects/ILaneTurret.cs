using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface ILaneTurret : IBaseTurret
    {
        TurretType Type { get; }
    }
}
