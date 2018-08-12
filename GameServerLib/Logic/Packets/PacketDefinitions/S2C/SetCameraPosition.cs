using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetCameraPosition : BasePacket
    {
        public SetCameraPosition(Champion champ, float x, float y, float z)
            : base(PacketCmd.PKT_S2C_SetCameraPosition, champ.NetId)
        {
            buffer.Write((float)x);
            buffer.Write((float)z); // Doesn't seem to matter
            buffer.Write((float)y);
        }
    }
}