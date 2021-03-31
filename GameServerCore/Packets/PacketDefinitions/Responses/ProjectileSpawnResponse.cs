using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ProjectileSpawnResponse : ICoreResponse
    {
        public int UserId { get; }
        public ISpellMissile Projectile { get; }
        public ProjectileSpawnResponse(int userId, ISpellMissile projectile)
        {
            UserId = userId;
            Projectile = projectile;
        }
    }
}