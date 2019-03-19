using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ProjectileDestroyResponse : ICoreResponse
    {
        public IProjectile Projectile { get; }
        public ProjectileDestroyResponse(IProjectile p)
        {
            Projectile = p;
        }
    }
};