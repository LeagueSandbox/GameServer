using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class RestrictCameraMovement : BasePacket
    {
        public RestrictCameraMovement(float x, float y, float z, float radius, bool enable)
            : base(PacketCmd.PKT_S2C_RESTRICT_CAMERA_MOVEMENT)
        {
            Write(x);
            Write(z);
            Write(y);
            Write(radius);
            Write(enable);
        }
    }
}