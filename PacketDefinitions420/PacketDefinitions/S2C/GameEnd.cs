using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class GameEnd : BasePacket
    {
        public GameEnd(bool winningTeamIsBlue)
            : base(PacketCmd.PKT_S2C_GAME_END)
        {
            Write(winningTeamIsBlue ? (byte)1 : (byte)0);
        }
    }
}