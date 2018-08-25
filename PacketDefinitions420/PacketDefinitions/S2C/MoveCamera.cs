using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class MoveCamera : BasePacket
    {
        public MoveCamera(IChampion champ, float x, float y, float z, float seconds)
            : base(PacketCmd.PKT_S2C_MOVE_CAMERA, champ.NetId)
        {
            // Unk, if somebody figures out let @horato know
            Write((byte)0x97);
            Write((byte)0xD4);
            Write((byte)0x00);
            Write((byte)0x58);
            Write((byte)0xD7);
            Write((byte)0x17);
            Write((byte)0x00);
            Write((byte)0xCD);
            Write((byte)0xED);
            Write((byte)0x13);
            Write((byte)0x01);
            Write((byte)0xA0);
            Write((byte)0x96);

            Write(x);
            Write(z); // I think this coordinate is ignored
            Write(y);
            Write(seconds);
        }
    }
}