using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class CloseGame : BasePacket
    {
        public CloseGame()
            : base(PacketCmd.PKT_S2_C_CLOSE_GAME)
        {
        }
    }
}