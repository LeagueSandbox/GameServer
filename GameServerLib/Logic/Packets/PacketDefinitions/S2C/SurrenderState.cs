using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SurrenderState : BasePacket
    {
        public SurrenderState(uint playernetid, byte state) 
            : base(PacketCmd.PKT_S2_C_SURRENDER_STATE, playernetid)
        {
            _buffer.Write(state);
        }
    }
}