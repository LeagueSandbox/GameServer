using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class RestrictCameraMovement : BasePacket
    {
        public RestrictCameraMovement(float x, float y, float z, float radius, bool enable)
            : base(PacketCmd.PKT_S2_C_RESTRICT_CAMERA_MOVEMENT)
        {
            _buffer.Write((float)x);
            _buffer.Write((float)z);
            _buffer.Write((float)y);
            _buffer.Write((float)radius);
            _buffer.Write(enable);
        }
    }
}