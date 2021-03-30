using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ProjectileShowResponse : ICoreResponse
    {
        public ISpellMissile Projectile { get; }
        public ProjectileShowResponse(ISpellMissile projectile)
        {
            Projectile = projectile;
        }
    }
}