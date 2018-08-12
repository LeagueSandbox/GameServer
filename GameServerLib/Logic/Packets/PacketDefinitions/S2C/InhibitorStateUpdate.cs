using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class InhibitorStateUpdate : BasePacket
    {
        public InhibitorStateUpdate(Inhibitor inhi)
            : base(PacketCmd.PKT_S2C_InhibitorState, inhi.NetId)
        {
            buffer.Write((byte)inhi.getState());
            buffer.Write((byte)0);
            buffer.Write((byte)0);
        }
    }
}