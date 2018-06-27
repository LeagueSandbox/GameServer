using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FaceDirection : BasePacket
    {
        public FaceDirection(AttackableUnit u,
            float relativeX,
            float relativeY,
            float relativeZ,
            bool instantTurn = true,
            float turnTime = 0.0833f)
            : base(PacketCmd.PKT_S2_C_FACE_DIRECTION, u.NetId)
        {
            _buffer.Write((byte)(instantTurn ? 0x00 : 0x01));
            _buffer.Write(relativeX);
            _buffer.Write(relativeZ);
            _buffer.Write(relativeY);
            _buffer.Write((float)turnTime);
        }
    };
}