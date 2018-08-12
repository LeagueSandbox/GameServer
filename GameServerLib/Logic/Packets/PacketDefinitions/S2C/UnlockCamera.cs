using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class UnlockCamera : BasePacket
    {
        public UnlockCamera(Game game)
            : base(PacketCmd.PKT_S2C_UNLOCK_CAMERA)
        {

        }
    }
}