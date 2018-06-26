using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class UnpauseGame : BasePacket
    {
        public UnpauseGame(uint unpauserNetId, bool showWindow) 
            : base(PacketCmd.PKT_UNPAUSE_GAME)
        {
            _buffer.Write((uint)unpauserNetId);
            _buffer.Write(showWindow);
        }
    }
}