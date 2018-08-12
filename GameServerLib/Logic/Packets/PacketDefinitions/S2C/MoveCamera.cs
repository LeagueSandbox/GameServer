using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class MoveCamera : BasePacket
    {
        public MoveCamera(Champion champ, float x, float y, float z, float seconds)
            : base(PacketCmd.PKT_S2C_MoveCamera, champ.NetId)
        {
            // Unk, if somebody figures out let @horato know
            buffer.Write((byte)0x97);
            buffer.Write((byte)0xD4);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x58);
            buffer.Write((byte)0xD7);
            buffer.Write((byte)0x17);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xCD);
            buffer.Write((byte)0xED);
            buffer.Write((byte)0x13);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0xA0);
            buffer.Write((byte)0x96);

            buffer.Write((float)x);
            buffer.Write((float)z); // I think this coordinate is ignored
            buffer.Write((float)y);
            buffer.Write((float)seconds);
        }
    }
}