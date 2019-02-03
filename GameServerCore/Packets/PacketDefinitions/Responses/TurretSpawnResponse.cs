using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class TurretSpawnResponse : ICoreResponse
    {
        public int UserId { get; }
        public ILaneTurret Turret { get; }
        public TurretSpawnResponse(int userId, ILaneTurret turret)
        {
            UserId = userId;
            Turret = turret;
        }
    }
}