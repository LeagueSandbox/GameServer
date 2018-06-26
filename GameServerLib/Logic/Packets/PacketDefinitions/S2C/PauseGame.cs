using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class PauseGame : BasePacket
    {
        public PauseGame(int seconds, bool showWindow) 
            : base(PacketCmd.PKT_PAUSE_GAME)
        {
            // The following structure might be incomplete or wrong
            _buffer.Write((int)0);
            _buffer.Write((int)seconds);
            _buffer.Write((bool)showWindow);
        }
    }
}