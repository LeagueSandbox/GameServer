using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ProjectileShowResponse : ICoreResponse
    {
        public IProjectile Projectile { get; }
        public ProjectileShowResponse(IProjectile projectile)
        {
            Projectile = projectile;
        }
    }
}