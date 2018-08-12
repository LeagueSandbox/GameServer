using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetCapturePoint : BasePacket
    {
        public SetCapturePoint(AttackableUnit unit, byte capturePointId)
            : base(PacketCmd.PKT_S2C_SET_CAPTURE_POINT)
        {
            Write(capturePointId);
            WriteNetId(unit);
            Fill(0, 6);
        }
    }
}