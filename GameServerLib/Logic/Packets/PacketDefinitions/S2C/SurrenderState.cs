using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SurrenderState : BasePacket
    {
        public SurrenderState(Game game, uint playernetid, byte state)
            : base(game, PacketCmd.PKT_S2C_SURRENDER_STATE, playernetid)
        {
            Write(state);
        }
    }
}