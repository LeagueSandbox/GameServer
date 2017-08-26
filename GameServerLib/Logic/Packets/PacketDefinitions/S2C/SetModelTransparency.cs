using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetModelTransparency : BasePacket
    {
        public SetModelTransparency(Unit u, float transparency, float transitionTime)
            : base(PacketCmd.PKT_S2C_SetModelTransparency, u.NetId)
        {
            // Applied to Teemo's mushrooms for example
            buffer.Write((byte)0xDB); // Unk
            buffer.Write((byte)0x00); // Unk
            buffer.Write((float)transitionTime);
            buffer.Write((float)transparency); // 0.0 : fully transparent, 1.0 : fully visible
        }
    }
}