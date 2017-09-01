using System.Collections.Generic;
using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetAnimation : BasePacket
    {
        public SetAnimation(Unit u, List<string> animationPairs)
            : base(PacketCmd.PKT_S2C_SetAnimation, u.NetId)
        {
            buffer.Write((byte)(animationPairs.Count / 2));

            for (int i = 0; i < animationPairs.Count; i++)
            {
                buffer.Write((int)animationPairs[i].Length);
                foreach (var b in Encoding.Default.GetBytes(animationPairs[i]))
                    buffer.Write(b);
            }
        }
    }
}