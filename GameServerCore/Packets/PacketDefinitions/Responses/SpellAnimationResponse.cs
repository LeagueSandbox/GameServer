using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class SpellAnimationResponse : ICoreResponse
    {
        public IAttackableUnit Unit { get; }
        public string Animation { get; }
        public SpellAnimationResponse(IAttackableUnit u, string animation)
        {
            Unit = u;
            Animation = animation;
        }
    }
}