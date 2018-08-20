using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class CloseGame : BasePacket
    {
        public CloseGame()
            : base(PacketCmd.PKT_S2C_CLOSE_GAME)
        {
        }
    }
}