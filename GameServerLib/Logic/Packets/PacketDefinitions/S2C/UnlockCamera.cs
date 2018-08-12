using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class UnlockCamera : BasePacket
    {
        public UnlockCamera()
            : base(PacketCmd.PKT_S2C_UnlockCamera)
        {

        }
    }
}