using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class RestrictCameraMovement : BasePacket
    {
        public RestrictCameraMovement(float x, float y, float z, float radius, bool enable)
            : base(PacketCmd.PKT_S2C_RestrictCameraMovement)
        {
            buffer.Write((float)x);
            buffer.Write((float)z);
            buffer.Write((float)y);
            buffer.Write((float)radius);
            buffer.Write(enable);
        }
    }
}