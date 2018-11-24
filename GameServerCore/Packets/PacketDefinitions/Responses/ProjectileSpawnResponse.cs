using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ProjectileSpawnResponse : ICoreResponse
    {
        public int UserId { get; }
        public IProjectile Projectile { get; }
        public ProjectileSpawnResponse(int userId, IProjectile projectile)
        {
            UserId = userId;
            Projectile = projectile;
        }
    }
}