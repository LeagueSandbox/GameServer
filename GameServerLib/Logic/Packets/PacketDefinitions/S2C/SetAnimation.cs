using System.Collections.Generic;
using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetAnimation : BasePacket
    {
        public SetAnimation(AttackableUnit u, List<string> animationPairs)
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