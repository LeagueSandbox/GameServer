using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetCapturePoint : BasePacket
    {
        public SetCapturePoint(Unit unit, byte capturePointId) 
            : base(PacketCmd.PKT_S2C_SetCapturePoint)
        {
            buffer.Write((byte)capturePointId);
            buffer.Write(unit.NetId);
            buffer.fill(0, 6);
        }
    }
}