using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ProjectileDestroyResponse : ICoreResponse
    {
        public ISpellMissile Projectile { get; }
        public ProjectileDestroyResponse(ISpellMissile p)
        {
            Projectile = p;
        }
    }
};