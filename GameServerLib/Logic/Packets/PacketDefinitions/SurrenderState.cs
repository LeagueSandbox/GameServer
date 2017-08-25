using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class SurrenderState : BasePacket
    {
        public SurrenderState(uint playernetid, byte state) : base(PacketCmd.PKT_S2C_SurrenderState, playernetid)
        {
            buffer.Write((byte)state);
        }
    }
}