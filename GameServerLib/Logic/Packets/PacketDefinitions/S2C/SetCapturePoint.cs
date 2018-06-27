using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetCapturePoint : BasePacket
    {
        public SetCapturePoint(AttackableUnit unit, byte capturePointId)
            : base(PacketCmd.PKT_S2C_SET_CAPTURE_POINT)
        {
            _buffer.Write(capturePointId);
            _buffer.Write(unit.NetId);
            _buffer.Fill(0, 6);
        }
    }
}