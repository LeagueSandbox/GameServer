using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class EnableFow : BasePacket
    {
        public EnableFow(Game game, bool activate)
            : base(game, PacketCmd.PKT_S2C_ENABLE_FOW)
        {
            Write(activate ? 0x01 : 0x00);
        }
    }
}