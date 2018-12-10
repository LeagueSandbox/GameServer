using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ParticleSpawnResponse : ICoreResponse
    {
        public IParticle Particle { get; }
        public ParticleSpawnResponse(IParticle particle)
        {
            Particle = particle;
        }
    }
};