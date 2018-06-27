using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetCameraPosition : BasePacket
    {
        public SetCameraPosition(Champion champ, float x, float y, float z)
            : base(PacketCmd.PKT_S2_C_SET_CAMERA_POSITION, champ.NetId)
        {
            _buffer.Write(x);
            _buffer.Write(z); // Doesn't seem to matter
            _buffer.Write(y);
        }
    }
}