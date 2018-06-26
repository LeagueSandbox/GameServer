using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class GameTimerUpdate : BasePacket
    {
        public GameTimerUpdate(float fTime)
            : base(PacketCmd.PKT_S2_C_GAME_TIMER_UPDATE, 0)
        {
            _buffer.Write((float)fTime);
        }
    }
}