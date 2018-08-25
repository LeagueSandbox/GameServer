using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class UnlockCamera : BasePacket
    {
        public UnlockCamera()
            : base(PacketCmd.PKT_S2C_UNLOCK_CAMERA)
        {

        }
    }
}