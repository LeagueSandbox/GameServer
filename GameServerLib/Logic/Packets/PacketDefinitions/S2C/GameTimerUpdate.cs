using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class GameTimerUpdate : BasePacket
    {
        public GameTimerUpdate(Game game, float fTime)
            : base(game, PacketCmd.PKT_S2C_GAME_TIMER_UPDATE, 0)
        {
            Write(fTime);
        }
    }
}