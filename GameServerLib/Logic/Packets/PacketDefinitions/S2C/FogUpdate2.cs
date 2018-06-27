using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FogUpdate2 : BasePacket
    {
        public FogUpdate2(AttackableUnit unit, NetworkIdManager idManager) : base(PacketCmd.PKT_S2_C_FOG_UPDATE2, 0)
        {
            _buffer.Write((int)unit.Team);
            _buffer.Write((byte)0xFE);
            _buffer.Write((byte)0xFF);
            _buffer.Write((byte)0xFF);
            _buffer.Write((byte)0xFF);
            _buffer.Write(0);
            _buffer.Write(unit.NetId); // Fog Attached, when unit dies it disappears
            _buffer.Write(idManager.GetNewNetId()); //Fog NetID
            _buffer.Write(0);
            _buffer.Write(unit.X);
            _buffer.Write(unit.Y);
            _buffer.Write((float)2500);
            _buffer.Write(88.4f);
            _buffer.Write((float)130);
            _buffer.Write(1.0f);
            _buffer.Write(0);
            _buffer.Write((byte)199);
            _buffer.Write(unit.VisionRadius);
        }
    }
}