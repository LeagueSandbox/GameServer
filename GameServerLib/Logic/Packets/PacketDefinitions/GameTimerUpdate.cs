using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class GameTimerUpdate : BasePacket
    {
        public GameTimerUpdate(float fTime) : base(PacketCmd.PKT_S2C_GameTimerUpdate, 0)
        {
            buffer.Write((float)fTime);
        }
    }
}