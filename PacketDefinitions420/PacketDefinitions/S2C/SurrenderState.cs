using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SurrenderState : BasePacket
    {
        public SurrenderState(uint playernetid, byte state)
            : base(PacketCmd.PKT_S2C_SURRENDER_STATE, playernetid)
        {
            Write(state);
        }
    }
}