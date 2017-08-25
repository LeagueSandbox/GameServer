using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class CloseGame : BasePacket
    {
        public CloseGame() : base(PacketCmd.PKT_S2C_CloseGame)
        {
        }
    }
}