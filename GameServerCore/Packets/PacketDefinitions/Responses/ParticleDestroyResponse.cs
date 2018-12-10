using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ParticleDestroyResponse : ICoreResponse
    {
        public IParticle Particle { get; }
        public ParticleDestroyResponse(IParticle particle)
        {
            Particle = particle;
        }
    }
};