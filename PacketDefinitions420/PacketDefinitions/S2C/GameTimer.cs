using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class GameTimer : BasePacket
    {
        public GameTimer(float fTime)
            : base(PacketCmd.PKT_S2C_GAME_TIMER, 0)
        {
            Write(fTime);
        }
    }
}