using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class PauseGame : BasePacket
    {
        public PauseGame(int seconds, bool showWindow) 
            : base(PacketCmd.PKT_PauseGame)
        {
            // The following structure might be incomplete or wrong
            buffer.Write((int)0);
            buffer.Write((int)seconds);
            buffer.Write((bool)showWindow);
        }
    }
}