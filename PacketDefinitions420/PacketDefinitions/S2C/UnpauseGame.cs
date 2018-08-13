using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class UnpauseGame : BasePacket
    {
        public UnpauseGame(uint unpauserNetId, bool showWindow)
            : base(PacketCmd.PKT_UNPAUSE_GAME)
        {
            Write(unpauserNetId);
            Write(showWindow);
        }
    }
}