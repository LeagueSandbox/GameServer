using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class UnpauseGame : BasePacket
    {
        public UnpauseGame(uint unpauserNetId, bool showWindow) : base(PacketCmd.PKT_UnpauseGame)
        {
            buffer.Write((uint)unpauserNetId);
            buffer.Write(showWindow);
        }
    }
}