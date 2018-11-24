using GameServerCore.Domain.GameObjects;
using System.Collections.Generic;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class SetAnimationResponse : ICoreResponse
    {
        public IAttackableUnit Unit { get; }
        public List<string> AnimationPairs { get; }
        public SetAnimationResponse(IAttackableUnit u, List<string> animationPairs)
        {
            Unit = u;
            AnimationPairs = animationPairs;
        }
    }
}