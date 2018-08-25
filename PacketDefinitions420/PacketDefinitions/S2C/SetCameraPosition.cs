using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SetCameraPosition : BasePacket
    {
        public SetCameraPosition(IChampion champ, float x, float y, float z)
            : base(PacketCmd.PKT_S2C_SET_CAMERA_POSITION, champ.NetId)
        {
            Write(x);
            Write(z); // Doesn't seem to matter
            Write(y);
        }
    }
}