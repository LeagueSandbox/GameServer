using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class RemoveMessageBoxRight : BasePacket
    {
        public RemoveMessageBoxRight() 
            : base(PacketCmd.PKT_S2C_RemoveMessageBoxRight)
        {
            // The following structure might be incomplete or wrong
        }
    }
}