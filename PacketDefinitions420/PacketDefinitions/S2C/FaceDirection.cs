using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class FaceDirection : BasePacket
    {
        public FaceDirection(IAttackableUnit u,
            float relativeX,
            float relativeY,
            float relativeZ,
            bool instantTurn = true,
            float turnTime = 0.0833f)
            : base(PacketCmd.PKT_S2C_FACE_DIRECTION, u.NetId)
        {
            Write((byte)(instantTurn ? 0x00 : 0x01));
            Write(relativeX);
            Write(relativeZ);
            Write(relativeY);
            Write(turnTime);
        }
    }
}