using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FaceDirection : BasePacket
    {
        public FaceDirection(Unit u,
            float relativeX,
            float relativeY,
            float relativeZ,
            bool instantTurn = true,
            float turnTime = 0.0833f)
            : base(PacketCmd.PKT_S2C_FaceDirection, u.NetId)
        {
            buffer.Write((byte)(instantTurn ? 0x00 : 0x01));
            buffer.Write(relativeX);
            buffer.Write(relativeZ);
            buffer.Write(relativeY);
            buffer.Write((float)turnTime);
        }
    };
}