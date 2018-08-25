using System.Collections.Generic;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SetAnimation : BasePacket
    {
        public SetAnimation(IAttackableUnit u, List<string> animationPairs)
            : base(PacketCmd.PKT_S2C_SET_ANIMATION, u.NetId)
        {
            Write((byte)(animationPairs.Count / 2));

            foreach (var t in animationPairs)
            {
                Write(t.Length);
                Write(t);
            }
        }
    }
}