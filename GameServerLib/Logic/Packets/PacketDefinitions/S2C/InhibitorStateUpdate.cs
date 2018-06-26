using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class InhibitorStateUpdate : BasePacket
    {
        public InhibitorStateUpdate(Inhibitor inhi)
            : base(PacketCmd.PKT_S2_C_INHIBITOR_STATE, inhi.NetId)
        {
            _buffer.Write((byte)inhi.GetState());
            _buffer.Write((byte)0);
            _buffer.Write((byte)0);
        }
    }
}