using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class MoveCamera : BasePacket
    {
        public MoveCamera(Champion champ, float x, float y, float z, float seconds)
            : base(PacketCmd.PKT_S2_C_MOVE_CAMERA, champ.NetId)
        {
            // Unk, if somebody figures out let @horato know
            _buffer.Write((byte)0x97);
            _buffer.Write((byte)0xD4);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x58);
            _buffer.Write((byte)0xD7);
            _buffer.Write((byte)0x17);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0xCD);
            _buffer.Write((byte)0xED);
            _buffer.Write((byte)0x13);
            _buffer.Write((byte)0x01);
            _buffer.Write((byte)0xA0);
            _buffer.Write((byte)0x96);

            _buffer.Write(x);
            _buffer.Write(z); // I think this coordinate is ignored
            _buffer.Write(y);
            _buffer.Write(seconds);
        }
    }
}